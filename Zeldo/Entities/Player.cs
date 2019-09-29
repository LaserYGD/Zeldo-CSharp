using System;
using Engine;
using Engine.Core;
using Engine.Interfaces._3D;
using Engine.Physics;
using Engine.Sensors;
using Engine.Shapes._3D;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.LinearMath;
using Newtonsoft.Json.Linq;
using Zeldo.Control;
using Zeldo.Entities.Core;
using Zeldo.Entities.Grabbable;
using Zeldo.Entities.Weapons;
using Zeldo.Interfaces;
using Zeldo.Items;
using Zeldo.Physics;
using Zeldo.Settings;
using Zeldo.UI;
using Zeldo.UI.Hud;
using Zeldo.View;

namespace Zeldo.Entities
{
	public class Player : Actor
	{
		private const int AscendIndex = (int)PlayerSkills.Ascend;
		private const int JumpIndex = (int)PlayerSkills.Jump;
		private const int DoubleJumpIndex = (int)PlayerSkills.DoubleJump;

		// This value should never be changed normally, but could theoretically be modified after release to give
		// additional double jumps (for something like a randomizer).
		private const int TargetJumps = 2;
		
		private PlayerData playerData;
		private PlayerControls controls;
		private PlayerController controller;
		private PlayerHealthDisplay healthDisplay;
		private DebugView debugView;
		private Sensor sensor;
		private Weapon weapon;

		private AerialController aerialController;
		private LadderController ladderController;
		private SurfaceController surfaceController;

		private IInteractive interactionTarget;
		private IAscendable ascensionTarget;
		
		private bool[] skillsUnlocked;
		private bool[] skillsEnabled;

		// This is kept as a separate boolean rather than a new state (to simplify those states).
		private bool isJumpDecelerating;

		// The game has double jumping, but is coded to accommodate any number of extra jumps.
		private int jumpsRemaining;

		private vec2 facing;

		public Player(ControlSettings settings) : base(EntityGroups.Player)
		{
			controls = new PlayerControls();
			playerData = new PlayerData();

			int skillCount = Utilities.EnumCount<PlayerSkills>();

			skillsUnlocked = new bool[skillCount];
			skillsEnabled = new bool[skillCount];
			controller = new PlayerController(this, playerData, controls, settings, CreateControllers());
			facing = vec2.UnitX;

			Swap(aerialController);
		}
		
		// This is required to move in the direction of camera aim (passed through to the controller class).
		public FollowController FollowController
		{
			set => controller.FollowController = value;
		}

		// The player owns their own inventory.
		public Inventory Inventory { get; }
		public Weapon Weapon => weapon;

		// States are used by the controller class to more easily determine when to apply certain actions.
		public PlayerStates State { get; private set; }

		// This is used by the player controller.
		public bool[] SkillsEnabled => skillsEnabled;
		public bool IsBlocking { get; private set; }
		public bool IsOnLadder => ladderController.Ladder != null;

		public int JumpsRemaining => jumpsRemaining;

		private AbstractController[] CreateControllers()
		{
			// Running
			float runAcceleration = Properties.GetFloat("player.run.acceleration");
			float runDeceleration = Properties.GetFloat("player.run.deceleration");
			float runMaxSpeed = Properties.GetFloat("player.run.max.speed");

			// Ladders
			float ladderAcceleration = Properties.GetFloat("player.ladder.climb.acceleration");
			float ladderDeceleration = Properties.GetFloat("player.ladder.climb.deceleration");
			float ladderMaxSpeed = Properties.GetFloat("player.ladder.climb.max.speed");
			float ladderDistance = Properties.GetFloat("player.ladder.distance");

			// Ladder climb distance is defined as the spacing between the player capsule and the ladder's body.
			float radius = Properties.GetFloat("player.capsule.radius");

			// Create controllers.
			aerialController = new AerialController(this);
			aerialController.Acceleration = runAcceleration;
			aerialController.Deceleration = runDeceleration;
			aerialController.MaxSpeed = runMaxSpeed;

			ladderController = new LadderController(this);
			ladderController.ClimbAcceleration = ladderAcceleration;
			ladderController.ClimbDeceleration = ladderDeceleration;
			ladderController.ClimbMaxSpeed = ladderMaxSpeed;

			// TODO: Use half ladder depth (rather than hardcoded).
			ladderController.ClimbDistance = ladderDistance + radius + 0.05f;

			surfaceController = new SurfaceController(this);

			var controllers = new AbstractController[4];
			controllers[ControllerIndexes.Aerial] = aerialController;
			controllers[ControllerIndexes.Ladder] = ladderController;
			controllers[ControllerIndexes.Surface] = surfaceController;

			return controllers;
		}

		public override void Initialize(Scene scene, JToken data)
		{
			// The height here is the height of the cylinder (excluding the two rounded caps).
			var capsuleHeight = Properties.GetFloat("player.capsule.height");
			var capsuleRadius = Properties.GetFloat("player.capsule.radius");

			Height = capsuleHeight + capsuleRadius * 2;

			CreateModel(scene, "Capsule.obj");

			var body = CreateKinematicBody(scene, new CapsuleShape(capsuleHeight, capsuleRadius));
			body.AllowDeactivation = false;

			// TODO: Should all actor sensors be axis-aligned?
			var shape = new Cylinder(Height, capsuleRadius);
			shape.IsAxisAligned = true;

			sensor = CreateSensor(scene, shape, SensorGroups.Player);

			var canvas = scene.Canvas;
			healthDisplay = canvas.GetElement<PlayerHealthDisplay>();
			debugView = canvas.GetElement<DebugView>();

			base.Initialize(scene, data);
		}

		// TODO: Move some of this code to the base Actor class as well.
		public override void OnCollision(Entity entity, vec3 point, vec3 normal, float penetration)
		{
			// TODO: Handle vaulting when near the top of a body.
			// The player can attach to ladders by jumping towards them (but only from one side).
			if (!onGround && entity is Ladder ladder && IsFacing(ladder))
			{
				Mount(ladder);

				return;
			}

			if (onGround && entity.IsStatic)
			{
				// TODO: Process other kinds of collisions against static entities (steps, vaults, wall presses, etc.).
				OnGroundedWallCollision(normal, penetration);
			}
		}

		// TODO: Move some of this code down to the base Actor class (so that other actors can properly traverse the environment too).
		public override void OnCollision(vec3 p, vec3 normal, vec3[] triangle, float penetration)
		{
			var surface = new SurfaceTriangle(triangle, normal, 0);

			// This situation can only occur if the triangle represents a wall or ceiling (since triangles flat enough
			// to be considered floors are ignored while an actor is grounded).
			if (onGround)
			{
				bool isStep = p.y - GroundPosition.y <= playerData.StepThreshold;

				// TODO: Should probably override ShouldIgnore instead (to negate the step collision entirely).
				if (isStep)
				{
					return;
				}

				OnGroundedWallCollision(normal, penetration);

				// If the player hits a wall with a velocity close to perpendicular, the player stops and presses
				// against the wall. Once in that state, the player will remain pressed until velocity moves outside
				// a small angle threshold.
				float angleN = Utilities.Angle(normal.swizzle.xz);
				float angleV = Utilities.Angle(controllingBody.LinearVelocity.ToVec3().swizzle.xz);

				// TODO: Verify that the wall isn't a vault target.
				// TODO: Verify that the collision point is wide enough to press (i.e. not a glancing hit).
				if (Utilities.Delta(angleN, angleV) <= playerData.WallPressThreshold)
				{
					PressAgainstWall();
				}

				return;
			}

			// While the sliding threshold represents a lower bound (the shallowest slope on which the player will
			// begin to slide), the wall thresholds are small and represent the maximum delta against a perfectly
			// vertical wall where the surface still counts as a wall. The upper threshold allows wall jumps off
			// surfaces that very slightly overhang, while the lower limit is a bit more generous and allows wall
			// interaction with very steep, but still upward-facing triangles.
			float slope = surface.Slope;

			bool isUpward = normal.y > 0;

			// The collision is against a wall.
			if (slope == 1 || (isUpward && slope >= 1 - playerData.WallLowerThreshold) || (!isUpward &&
				slope <= 1 - playerData.WallUpperThreshold))
			{
				return;
			}

			// The collision is a landing on a surface flat enough to run or slide.
			if (isUpward)
			{
				OnLanding(p, surface);
			}
		}

		// Note that the position given is the ground position.
		private void OnLanding(vec3 p, SurfaceTriangle surface)
		{
			// Project onto the surface. Note that setting ground position *before* the onGround flag, the body's
			// velocity isn't wastefully set twice (since it's forcibly set to zero below).
			surface.Project(p, out vec3 result);
			GroundPosition = result;
			onGround = true;
			
			RefreshJumps();

			// TODO: Account for speed differences when landing on slopes (since maximum flat speed will be a bit lower). Maybe quick deceleration?
			// The surface controller updates the body's velocity, so that velocity needs to be transferred here first.
			var bodyVelocity = controllingBody.LinearVelocity;
			var v = SurfaceVelocity;
			v.x = bodyVelocity.X;
			v.z = bodyVelocity.Z;
			SurfaceVelocity = v;

			controller.OnLanding(surface);
			controllingBody.AffectedByGravity = false;

			// TODO: Setting body position directly could cause rare collision misses on dynamic objects. Should be tested.
			controllingBody.Position = (result + new vec3(0, Height / 2, 0)).ToJVector();
			controllingBody.LinearVelocity = JVector.Zero;

			OnSurfaceTransition(surface);
			Swap(null);
		}

		public void OnSurfaceTransition(SurfaceTriangle surface)
		{
			// Moving to a surface flat enough for normal running.
			if (surface.Slope < playerData.SlideThreshold)
			{
				State = PlayerStates.Running;

				return;
			}

			// Moving to a surface steep enough to cause sliding.
			State = PlayerStates.Sliding;
		}

		private void OnGroundedWallCollision(vec3 normal, float penetration)
		{
			Position += normal * penetration;
			isSurfaceControlOverridden = true;

			if (Utilities.Dot(SurfaceVelocity, normal) < 0)
			{
				SurfaceVelocity -= Utilities.Project(SurfaceVelocity, normal);
			}
		}

		private void PressAgainstWall()
		{
		}

		// This is called when the player runs or walks off an edge (without jumping).
		public void BecomeAirborneFromLedge()
		{
			onGround = false;
			controllingBody.LinearVelocity = SurfaceVelocity.ToJVector();
			controllingBody.AffectedByGravity = true;
			jumpsRemaining--;
			skillsEnabled[JumpIndex] = false;

			Swap(aerialController);
		}

		public void Jump()
		{
			// Jumps are decremented regardless of single vs. double jump.
			jumpsRemaining--;

			// The player can jump off ladders.
			if (IsOnLadder)
			{
				ladderController.Ladder = null;
			}

			if (!skillsUnlocked[DoubleJumpIndex] || jumpsRemaining == TargetJumps - 1)
			{
				SingleJump();
			}
			else
			{
				DoubleJump();
			}

			State = PlayerStates.Jumping;
		}

		private void SingleJump()
		{
			// TODO: Set velocity accordingly when jumping from different states (like climbing a ladder).
			// On jump, the controlling body inherits surface velocity.
			controllingBody.LinearVelocity = new JVector(SurfaceVelocity.x, playerData.JumpSpeed, SurfaceVelocity.z);
			controllingBody.AffectedByGravity = true;

			var v = controllingBody.LinearVelocity;
			v.Y = playerData.JumpSpeed;

			// This single jump function can be triggered from multiple scenarios (including normal jumps off the
			// ground, jumping off ladders and ropes, or jumping from an ascend).
			if (onGround)
			{
				v.X = SurfaceVelocity.x;
				v.Z = SurfaceVelocity.z;
			}
			else
			{
			}

			controllingBody.LinearVelocity = v;
			onGround = false;
			skillsEnabled[JumpIndex] = false;

			Swap(aerialController);
		}

		private void DoubleJump()
		{
			skillsEnabled[DoubleJumpIndex] = jumpsRemaining > 0;

			var v = controllingBody.LinearVelocity;
			v.Y = playerData.DoubleJumpSpeed;
			controllingBody.LinearVelocity = v;
		}

		// This function is only called when limiting actually has to occur (i.e. the player's velocity is checked in
		// advance).
		public void LimitJump(float dt)
		{
			if (!DecelerateJump(dt))
			{
				isJumpDecelerating = true;
			}
			
			State = PlayerStates.Airborne;
		}

		private void RefreshJumps()
		{
			var djUnlocked = skillsUnlocked[DoubleJumpIndex];

			skillsEnabled[JumpIndex] = skillsUnlocked[JumpIndex];
			skillsEnabled[DoubleJumpIndex] = djUnlocked;
			jumpsRemaining = djUnlocked ? TargetJumps : 1;
			isJumpDecelerating = false;
		}

		public bool TryAscend()
		{
			// The player can ascend while climbing ladders normally.
			if (IsOnLadder)
			{
				Ascend(ladderController.Ladder);

				return true;
			}

			foreach (var contact in sensor.Contacts)
			{
				if (contact.Owner is IAscendable target)
				{
					Ascend(target);

					return true;
				}
			}

			return false;
		}

		private void Ascend(IAscendable target)
		{
			ascensionTarget = target;
			State = PlayerStates.Ascending;
			skillsEnabled[AscendIndex] = false;
			
			RefreshJumps();
		}

		public void BreakAscend()
		{
			// Breaking out of an ascend uses variable height as well, meaning that similar jump logic can be reused.
			State = PlayerStates.Jumping;
		}

		public bool TryGrab()
		{
			// The player must be facing the target in order to grab it.
			if (!sensor.GetContact(out IGrabbable target) || !IsFacing(target))
			{
				return false;
			}

			// TODO: Play the appropriate grab animation.
			State = PlayerStates.Grabbing;

			return true;
		}

		public void ReleaseGrab()
		{
		}

		public void TryInteract()
		{
			// TODO: Handle multiple interactive targets (likely with a swap, similar to Dark Souls).
			if (!sensor.GetContact(out IInteractive target) || !target.IsInteractionEnabled)
			{
				return;
			}

			if (!target.RequiresFacing || IsFacing(target))
			{
				target.OnInteract(this);
			}
		}

		private bool IsFacing(IPositionable3D target)
		{
			// TODO: Should probably narrow the spread that counts as facing a target.
			return Utilities.Dot(target.Position.swizzle.xz - position.swizzle.xz, facing) > 0;
		}

		public void Equip(Weapon weapon)
		{
			this.weapon = weapon;

			weapon.Owner = this;
		}

		public void Block()
		{
			IsBlocking = true;
		}

		public void Unblock()
		{
			IsBlocking = false;
		}

		public void Parry()
		{
			// TODO: Check player direction (to see if the parry should trigger).
		}

		public void Mount(Ladder ladder)
		{
			// TODO: Whip around as appropriate.
			Proximities proximity = ladder.ComputeProximity(position);

			ladderController.OnMount(ladder, this);
			Swap(ladderController);
			RefreshJumps();

			controllingBody.AffectedByGravity = false;
		}

		public void UnlockSkill(PlayerSkills skill)
		{
			int index = (int)skill;

			skillsUnlocked[index] = true;
			skillsEnabled[index] = IsSkillEnabledOnUnlock(skill);
		}

		private bool IsSkillEnabledOnUnlock(PlayerSkills skill)
		{
			switch (skill)
			{
				case PlayerSkills.Grab: return false;
				case PlayerSkills.Jump: return onGround;
			}

			return true;
		}

		public override void Update(float dt)
		{
			// TODO: Add an isOrientationFixed boolean to rigid bodies and use that instead.
			controllingBody.Orientation = JMatrix.Identity;

			if (isJumpDecelerating && DecelerateJump(dt))
			{
				isJumpDecelerating = false;
			}
			else switch (State)
			{
				case PlayerStates.Jumping when controllingBody.LinearVelocity.Y <= playerData.JumpLimit:
					State = PlayerStates.Airborne;
					break;

				case PlayerStates.Ascending:
					UpdateAscend(dt);
					break;
			}

			var v = controllingBody.LinearVelocity.ToVec3();
			var entries = new []
			{
				$"Position: {Position.x:N2}, {Position.y:N2}, {Position.z:N2}",
				$"Old position: {oldPosition.x:N2}, {oldPosition.y:N2}, {oldPosition.z:N2}",
				$"Surface velocity: {SurfaceVelocity.x:N2}, {SurfaceVelocity.y:N2}, {SurfaceVelocity.z:N2}",
				$"Body velocity: {v.x:N2}, {v.y:N2}, {v.z:N2}",
				$"Body gravity: {controllingBody.AffectedByGravity}",
				$"On ground: {onGround}",
				$"Jump enabled: {skillsEnabled[JumpIndex]}",
				$"DJ enabled: {skillsEnabled[DoubleJumpIndex]}",
				$"Jump decelerating: {isJumpDecelerating}",
				$"Jumps remaining: {jumpsRemaining}",
				$"State: {State}"
			};

			debugView.GetGroup("Player").AddRange(entries);

			base.Update(dt);

			// TODO: This logic should be re-examined (or maybe applied to all actors).
			if (position.x != oldPosition.x || position.z != oldPosition.z)
			{
				facing = Utilities.Normalize((position - oldPosition).swizzle.xz);
			}

			Scene.DebugPrimitives.DrawLine(Position, Position + new vec3(facing.x, 0, facing.y), Color.Cyan);
		}

		private bool DecelerateJump(float dt)
		{
			var v = controllingBody.LinearVelocity;
			var limit = playerData.JumpLimit;

			v.Y -= playerData.JumpDeceleration * dt;

			if (v.Y <= limit)
			{
				v.Y = limit;
				isJumpDecelerating = false;

				// Returning true means that the jump has finished decelerating.
				return true;
			}

			controllingBody.LinearVelocity = v;

			return false;
		}

		private void UpdateAscend(float dt)
		{
			// The body's linear velocity is reused for ascension.
			var v = controllingBody.LinearVelocity;
			v.Y += playerData.AscendAcceleration * dt;

			if (v.Y > playerData.AscendTargetSpeed)
			{
				v.Y = playerData.AscendTargetSpeed;
			}

			controllingBody.LinearVelocity = v;
		}

		public static class ControllerIndexes
		{
			public const int Aerial = 0;
			public const int Ladder = 1;
			public const int Surface = 2;
			public const int Swim = 3;
		}
	}
}
