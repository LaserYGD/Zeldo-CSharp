using System;
using Engine;
using Engine.Input.Data;
using Engine.Physics;
using Engine.Shapes._2D;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Newtonsoft.Json.Linq;
using Zeldo.Control;
using Zeldo.Entities.Core;
using Zeldo.Entities.Weapons;
using Zeldo.Interfaces;
using Zeldo.Items;
using Zeldo.Physics;
using Zeldo.Sensors;
using Zeldo.UI;
using Zeldo.UI.Hud;
using Zeldo.View;

namespace Zeldo.Entities
{
	public class Player : Actor
	{
		private const int DashIndex = (int)PlayerSkills.Dash;
		private const int GrabIndex = (int)PlayerSkills.Grab;
		private const int JumpIndex = (int)PlayerSkills.Jump;
		
		private Sensor sensor;
		private PlayerData playerData;
		private PlayerControls controls;
		private PlayerController controller;
		private Weapon weapon;

		private AerialController aerialController;
		private SurfaceController surfaceController;
		
		private bool[] skillsUnlocked;
		private bool[] skillsEnabled;

		// This is kept as a separate boolean rather than a new state (to simplify those states).
		private bool isJumpDecelerating;

		public Player() : base(EntityGroups.Player)
		{
			controls = new PlayerControls();
			playerData = new PlayerData();

			int skillCount = Utilities.EnumCount<PlayerSkills>();

			skillsUnlocked = new bool[skillCount];
			skillsEnabled = new bool[skillCount];

			float acceleration = Properties.GetFloat("player.run.acceleration");
			float deceleration = Properties.GetFloat("player.run.deceleration");
			float maxSpeed = Properties.GetFloat("player.run.max.speed");

			// Create controllers.
			aerialController = new AerialController(this);
			aerialController.Acceleration = acceleration;
			aerialController.Deceleration = deceleration;
			aerialController.MaxSpeed = maxSpeed;

			surfaceController = new SurfaceController(this);

			var controllers = new AbstractController[2];
			controllers[ControllerIndexes.Aerial] = aerialController;
			controllers[ControllerIndexes.Surface] = surfaceController;

			controller = new PlayerController(this, playerData, controls, controllers);
			Swap(aerialController);
		}
		
		public PlayerHealthDisplay HealthDisplay { get; set; }
		public PlayerManaDisplay ManaDisplay { get; set; }
		public PlayerDebugView DebugView { get; set; }

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

		public override void Initialize(Scene scene, JToken data)
		{
			// The height here is the height of the cylinder (excluding the two rounded caps).
			var capsuleHeight = Properties.GetFloat("player.capsule.height");
			var capsuleRadius = Properties.GetFloat("player.capsule.radius");

			Height = capsuleHeight + capsuleRadius * 2;

			CreateModel(scene, "Capsule.obj");
			CreateKinematicBody(scene, new CapsuleShape(capsuleHeight, capsuleRadius));

			//sensor = CreateSensor(scene, groundShape, SensorUsages.Hitbox | SensorUsages.Interaction, Height);
			//CreateSensor(scene, new Point(), SensorUsages.Control, 1, null, -0.75f);

			base.Initialize(scene, data);
		}

		// TODO: Move some of this code to the base Actor class as well.
		public override void OnCollision(Entity entity, vec3 point, vec3 normal, float penetration)
		{
			if (onGround && entity.IsStatic)
			{
				Position += normal * penetration;
			}
		}

		// TODO: Move some of this code down to the base Actor class (so that other actors can properly traverse the environment too).
		public override void OnCollision(vec3 p, vec3 normal, vec3[] triangle)
		{
			// This fixes a "fake" collision that occurs when the player jumps and separates from a surface.
			// TODO: If the surface is a moving platform, the body's Y velocity will need to be compared against the platform.
			// TODO: This fake collision might be due to using capsules on a slope (such that it technically intersects the surface a bit when set by bottom point).
			if (!onGround && Utilities.Dot(normal, vec3.UnitY) > 0 && controllingBody.LinearVelocity.Y > 0)
			{
				return;
			}

			var surface = new SurfaceTriangle(triangle, normal, 0);

			// TODO: Process running into static objects while grounded.
			if (onGround)
			{
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
			if (slope == 1 || (isUpward && slope >= 1 - playerData.LowerWallThreshold) || (!isUpward &&
				slope <= 1 - playerData.UpperWallThreshold))
			{
				return;
			}

			// The collision is a landing on a surface flat enough to run or slide.
			if (isUpward)
			{
				OnLanding(p, surface);
			}
		}

		private void OnLanding(vec3 p, SurfaceTriangle surface)
		{
			onGround = true;
			isJumpDecelerating = false;
			skillsEnabled[JumpIndex] = skillsUnlocked[JumpIndex];

			// TODO: Account for speed differences when landing on slopes (since maximum flat speed will be a bit lower). Maybe quick deceleration?
			// The surface controller updates the body's velocity, so that velocity needs to be transferred here first.
			var bodyVelocity = controllingBody.LinearVelocity;
			var v = SurfaceVelocity;
			v.x = bodyVelocity.X;
			v.z = bodyVelocity.Z;
			SurfaceVelocity = v;

			controller.OnLanding(p, surface);
			controllingBody.AffectedByGravity = false;

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

		public void Jump()
		{
			onGround = false;
			skillsEnabled[JumpIndex] = false;
			State = PlayerStates.Jumping;
			
			// On jump, the controlling body inherits surface velocity.
			controllingBody.LinearVelocity = new JVector(SurfaceVelocity.x, playerData.JumpSpeed, SurfaceVelocity.z);
			controllingBody.AffectedByGravity = true;

			var v = controllingBody.LinearVelocity;
			v.X = SurfaceVelocity.x;
			v.Z = SurfaceVelocity.z;
			controllingBody.LinearVelocity = v;

			Swap(aerialController);
		}

		// This function is only called when limiting actually has to occur (i.e. the player's velocity is checked in
		// advance).
		public void LimitJump(float dt)
		{
			if (!DecelerateJump(dt))
			{
				isJumpDecelerating = true;
			}

			// Note that if the player *doesn't* limit a jump (by releasing the jump bind early), the state will
			// remain Jumping (rather than Airborne) until it otherwise changes.
			State = PlayerStates.Airborne;
		}

		public void Ascend()
		{
			// TODO: Attach to the ascension target and begin climbing.
			State = PlayerStates.Ascending;
		}

		public void BreakAscend()
		{
			// Breaking out of an ascend uses variable height as well, meaning that similar jump logic can be reused.
			State = PlayerStates.Jumping;
		}

		public void Interact()
		{
			var contacts = sensor.Contacts;

			for (int i = contacts.Count - 1; i >= 0; i--)
			{
				if (contacts[i].Parent is IInteractive target && target.IsInteractionEnabled)
				{
					target.OnInteract(this);

					// Only one object can be interacted with each frame.
					return;
				}
			}
		}

		public void Equip(Weapon weapon)
		{
			this.weapon = weapon;

			weapon.Owner = this;
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

		public void GiveItem(int id, int count = 1)
		{
		}

		public override void Update(float dt)
		{
			// TODO: Add an isOrientationFixed boolean to rigid bodies and use that instead.
			controllingBody.Orientation = JMatrix.Identity;

			if (isJumpDecelerating && DecelerateJump(dt))
			{
				isJumpDecelerating = false;
			}

			var v = controllingBody.LinearVelocity.ToVec3();

			DebugView.Lines = new []
			{
				$"Position: {Position.x}, {Position.y}, {Position.z}",
				$"Surface velocity: {SurfaceVelocity.x}, {SurfaceVelocity.y}, {SurfaceVelocity.z}",
				$"Body velocity: {v.x}, {v.y}, {v.z}",
				$"On ground: {onGround}",
				$"Jump enabled: {skillsEnabled[JumpIndex]}",
				$"Jump decelerating: {isJumpDecelerating}",
				$"State: {State}"
			};

			base.Update(dt);
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

		public static class ControllerIndexes
		{
			public const int Aerial = 0;
			public const int Surface = 1;
			public const int Swimming = 2;
		}
	}
}
