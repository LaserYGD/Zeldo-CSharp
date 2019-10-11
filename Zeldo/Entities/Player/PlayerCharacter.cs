using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Core;
using Engine.Interfaces._3D;
using Engine.Physics;
using Engine.Sensors;
using Engine.Shapes._3D;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
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

namespace Zeldo.Entities.Player
{
	// TODO: Jumping is processed via jumpsRemaining, not the actual skill flags. Could consider optimizing this.
	public class PlayerCharacter : Actor
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

		// Flags
		private TimedFlag coyoteFlag;

		// Controllers
		private AerialController aerialController;
		private LadderController ladderController;

		private IInteractive interactionTarget;
		private IAscendable ascensionTarget;
		
		private bool[] skillsUnlocked;
		private bool[] skillsEnabled;

		// This is kept as a separate boolean rather than a new state (to simplify those states).
		private bool isJumpDecelerating;

		// The game has double jumping, but is coded to accommodate any number of extra jumps.
		private int jumpsRemaining;

		private vec2 facing;

		// While grounded, it's possible for actors to hit multiple walls at once. These collisions can result in
		// overlapping resolution vectors, which in turn can cause visual jitter as those vectors are applied. To fix
		// this, vectors are accumulated each frame, then resolved using custom logic during the update step.
		//private List<vec3> wallVectors;

		public PlayerCharacter(ControlSettings settings) : base(EntityGroups.Player)
		{
			controls = new PlayerControls();
			playerData = new PlayerData();

			int skillCount = Utilities.EnumCount<PlayerSkills>();

			// Flags
			coyoteFlag = Components.Add(new TimedFlag(playerData.CoyoteJumpTime, false));
			coyoteFlag.OnExpiration = () =>
			{
				jumpsRemaining--;
				skillsEnabled[JumpIndex] = false;
			};

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
			body.PreStep = PreStep;
			body.PostStep = PostStep;

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
			var onSurface = OnSurface;

			// TODO: Handle vaulting when near the top of a body.
			// The player can attach to ladders by jumping towards them (from any side).
			if (!onSurface && entity is Ladder ladder && IsFacing(ladder))
			{
				Mount(ladder);

				return;
			}

			if (onSurface && entity.IsStatic)
			{
				// TODO: Process other kinds of collisions against static entities (steps, vaults, wall presses, etc.).
				OnGroundedSurfaceCollision(normal, penetration);
			}
		}

		// TODO: Move some of this code down to the base Actor class (so that other actors can properly traverse the environment too).
		public override void OnCollision(vec3 p, vec3 normal, vec3[] triangle, float penetration)
		{
			var surface = new SurfaceTriangle(triangle, normal, 0);

			// This situation can only occur if the triangle represents a different kind of surface (e.g. while
			// running on the ground, you might hit a wall).
			if (OnSurface)
			{
				// Wall-pressing is processed first (which, by definition, can only occur from floors to walls).
				if (surface.SurfaceType == SurfaceTypes.Wall)
				{
					// TODO: Should probably override ShouldCollideWith instead (to negate the step collision entirely).
					bool isStep = p.y - GroundPosition.y <= playerData.StepThreshold;

					if (isStep)
					{
						return;
					}

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

						return;
					}
				}

				// This applies to both walls and low-hanging ceilings.
				//OnGroundedSurfaceCollision(normal, penetration);
			}
		}

		/*
		protected override bool ShouldCollideWith(RigidBody body, JVector[] triangle)
		{
			// Triangles are only sent into the callback for triangle mesh and terrain collisions. For now, collisions
			// are only ignored to accommodate surface movement (which also means that actors without a surface
			// controller created can return early).
			//if (triangle == null || surfaceController == null)
			if (triangle == null)
			{
				return true;
			}

			var onSurface = OnSurface;
			var surfaceType = SurfaceTriangle.ComputeSurfaceType(triangle, WindingTypes.CounterClockwise);

			bool isPotentialLanding = !onSurface && surfaceType == SurfaceTypes.Floor;

			// TODO: This will cause glancing downward collisions to be ignored. Should they be?
			// Since actors use capsules, potential ground collisions are ignored from the air. Instead, raycasts are
			// used to determine when the exact bottom-center of the capsule crosses a triangle.
			if (isPotentialLanding)
			{
				return false;
			}

			// While on a surface, only collisions with surfaces of *different* types are processed. For example,
			// while grounded, only wall and ceiling collisions should occur (the surface controller handles movement
			// among surfaces of the same type).
			if (!(onSurface && surfaceType != surfaceController.Surface.SurfaceType))
			{
				return false;
			}

			// This helps prevent phantom collisions while separating from a surface (or sliding along a corner).
			var n = Utilities.ComputeNormal(triangle[0], triangle[1], triangle[2], WindingTypes.CounterClockwise,
				false).ToVec3();

			return Utilities.Dot(controller.FlatDirection, n.swizzle.xz) < 0;
		}
		*/

		private void PreStep(float step)
		{
			if (!OnSurface)
			{
				return;
			}

			// TODO: Should velocity be applied here?
			controllingBody.LinearVelocity = controller.AdjustRunningVelocity(controller.FlatDirection, step).ToJVector();

			var vectors = new List<vec3>();
			var surfaceNormal = surfaceController.Surface.Normal;

			foreach (Arbiter arbiter in controllingBody.Arbiters)
			{
				var contacts = arbiter.ContactList;

				// Before the physics step occurs, all static contacts are aggregated together and manually applied
				// (based on surface normal).
				for (int i = contacts.Count - 1; i >= 0; i--)
				{
					var contact = contacts[i];
					var b1 = contact.Body1;
					var b2 = contact.Body2;

					if (!(b1.IsStatic || b2.IsStatic))
					{
						continue;
					}

					var n = contact.Normal.ToVec3();

					if (controllingBody == b1)
					{
						n *= -1;
					}

					var v = Utilities.Normalize(Utilities.ProjectOntoPlane(n, surfaceNormal));
					var angle = Utilities.Angle(n, v);
					var l = contact.Penetration / (float)Math.Cos(angle);

					vectors.Add(v * l);
					contacts.RemoveAt(i);
				}
			}

			if (vectors.Count > 0)
			{
				ResolveSurfaceVectors(vectors, step);
			}
		}

		private void ResolveSurfaceVectors(List<vec3> vectors, float step)
		{
			var final = vec3.Zero;

			for (int i = 0; i < vectors.Count; i++)
			{
				var v = vectors[i];
				final += v;

				for (int j = i + 1; j < vectors.Count; j++)
				{
					vectors[j] -= Utilities.Project(v, vectors[j]);
				}
			}

			if (Utilities.LengthSquared(final) > 0.001f)
			{
				var v = controllingBody.LinearVelocity.ToVec3();

				Position += v * step + final;
				controllingBody.LinearVelocity -= Utilities.Project(v, final).ToJVector();
			}
		}

		private void PostStep(float step)
		{
			if (!OnSurface)
			{
				return;
			}

			var p = controllingBody.Position.ToVec3() - new vec3(0, Height / 2, 0);
			var surface = surfaceController.Surface;

			// If the projection returns true, that means the actor is still within the current triangle.
			if (surface.Project(p, out vec3 result))
			{
				GroundPosition = result;

				return;
			}

			// TODO: Store a reference to the physics map separately (rather than querying the world every frame).
			var world = Scene.World;
			var map = world.RigidBodies.First(b => b.Shape is TriangleMeshShape);
			var normal = surface.Normal;

			// The raycast needs to be offset upward enough to catch steps.
			// TODO: Use properties for these raycast values.
			var results = PhysicsUtilities.Raycast(world, map, p + normal, -normal, 1.2f);

			// This means the actor moved to another triangle.
			if (results?.Triangle != null)
			{
				surface = new SurfaceTriangle(results.Triangle, results.Normal, 0);
				surface.Project(results.Position, out result);

				// TODO: Signal the actor of the surface transition (if needed).
				GroundPosition = result;
				OnSurfaceTransition(surface);

				return;
			}

			// If the actor has moved past a surface triangle (without transitioning to another one), a very small
			// forgiveness distance is checked before signalling the actor to become airborne. This distance is small
			// enough to not be noticeable during gameplay, but protects against potential floating-point errors near
			// the seams of triangles.
			// TODO: Use a constant.
			if (ComputeForgiveness(p, surface) > Properties.GetFloat("edge.forgiveness"))
			{
				BecomeAirborneFromLedge();
			}
		}

		private float ComputeForgiveness(vec3 p, SurfaceTriangle surface)
		{
			// To compute the shortest distance to an edge of the triangle, points are rotated to a flat plane first
			// (using the surface normal).
			var q = Utilities.Orientation(surface.Normal, vec3.UnitY);
			var flatP = (q * p).swizzle.xz;
			var flatPoints = surface.Points.Select(v => (q * v).swizzle.xz).ToArray();
			var d = float.MaxValue;

			for (int i = 0; i < flatPoints.Length; i++)
			{
				var p1 = flatPoints[i];
				var p2 = flatPoints[(i + 1) % 3];

				d = Math.Min(d, Utilities.DistanceToLine(flatP, p1, p2));
			}

			return d;
		}

		protected override void OnLanding(vec3 p, SurfaceTriangle surface)
		{
			base.OnLanding(p, surface);

			RefreshJumps();

			// TODO: Re-examine this (should all actors avoid the surface controller?)
			Swap(null);
		}

		public override void OnSurfaceTransition(SurfaceTriangle surface)
		{
			// TODO: Re-enable sliding later (if sliding is actually kept in the game).
			/*
			// Moving to a surface flat enough for normal running.
			if (surface.Slope < playerData.SlideThreshold)
			{
				State = PlayerStates.Running;

				return;
			}

			// Moving to a surface steep enough to cause sliding.
			State = PlayerStates.Sliding;
			*/

			base.OnSurfaceTransition(surface);
		}

		// TODO: Does this need to be moved down to Actor?
		private void OnGroundedSurfaceCollision(vec3 normal, float penetration)
		{
			// This helps ignore glancing collisions while sliding along another wall.
			if (Utilities.Dot(SurfaceVelocity, normal) >= 0)
			{
				return;
			}
			
			// Rather than resolve collisions using the wall normal, vectors are projected to resolve parallel to
			// the current surface. This approach prevents weird tunneling into the floor for walls that aren't
			// perfectly vertical.
			var v = Utilities.ProjectOntoPlane(normal, surfaceController.Surface.Normal);
			var angle = Utilities.Angle(normal, v);
			var l = penetration / (float)Math.Cos(angle);
			
			// This step occurs before the scene is updated. 
			Position += v * l;
			SurfaceVelocity -= Utilities.Project(SurfaceVelocity, v);
			isSurfaceControlOverridden = true;
		}

		private void PressAgainstWall()
		{
		}

		// This is called when the player runs or walks off an edge (without jumping).
		public override void BecomeAirborneFromLedge()
		{
			// TODO: Move some of this to the base class.
			surfaceController.Surface = null;
			controllingBody.IsAffectedByGravity = true;
			surfaceController.Surface = null;
			coyoteFlag.Refresh();

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
			controllingBody.IsAffectedByGravity = true;

			var v = controllingBody.LinearVelocity;
			v.Y = playerData.JumpSpeed;

			// This single jump function can be triggered from multiple scenarios (including normal jumps off the
			// ground, jumping off ladders and ropes, or jumping from an ascend).
			// TODO: Process jumps from other scenarios (like ropes and ascend).
			if (!OnSurface)
			{
			}

			controllingBody.LinearVelocity = v;
			surfaceController.Surface = null;
			skillsEnabled[JumpIndex] = false;
			coyoteFlag.Reset();

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

			// I'm pretty sure this logic is correct (whenever jumps are refreshed, the coyote flag should be reset as
			// well).
			coyoteFlag.Reset();
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

			controllingBody.IsAffectedByGravity = false;
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

				// TODO: Make sure this works if you're standing on an entity (rather than a mesh).
				case PlayerSkills.Jump: return OnSurface &&
					surfaceController.Surface.SurfaceType == SurfaceTypes.Floor;
			}

			return true;
		}

		public override void Update(float dt)
		{
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

			var p = controllingBody.Position.ToVec3();
			var v = controllingBody.LinearVelocity.ToVec3();
			var entries = new []
			{
				$"P Position: {Position.x:N2}, {Position.y:N2}, {Position.z:N2}",
				$"B position: {p.x:N2}, {p.y:N2}, {p.z:N2}",
				$"Old position: {oldPosition.x:N2}, {oldPosition.y:N2}, {oldPosition.z:N2}",
				$"Surface velocity: {SurfaceVelocity.x:N2}, {SurfaceVelocity.y:N2}, {SurfaceVelocity.z:N2}",
				$"Body velocity: {v.x:N2}, {v.y:N2}, {v.z:N2}",
				$"Flat direction: {controller.FlatDirection}",
				$"Arbiters: {controllingBody.Arbiters.Count}",
				$"Contacts: {controllingBody.Arbiters.Sum(a => a.ContactList.Count)}",
				$"On surface: {OnSurface}",
				$"Jumps remaining: {jumpsRemaining}"
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
