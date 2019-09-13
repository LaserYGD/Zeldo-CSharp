using Engine;
using Engine.Physics;
using Engine.Sensors;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.LinearMath;
using Newtonsoft.Json.Linq;
using Zeldo.Control;
using Zeldo.Entities.Core;
using Zeldo.Entities.Weapons;
using Zeldo.Interfaces;
using Zeldo.Items;
using Zeldo.Physics;
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
		private PlayerHealthDisplay healthDisplay;
		private DebugView debugView;
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
			CreateKinematicBody(scene, new CapsuleShape(capsuleHeight, capsuleRadius)).AllowDeactivation = false;

			//sensor = CreateSensor(scene, groundShape, SensorUsages.Hitbox | SensorUsages.Interaction, Height);
			//CreateSensor(scene, new Point(), SensorUsages.Control, 1, null, -0.75f);

			var canvas = scene.Canvas;
			healthDisplay = canvas.GetElement<PlayerHealthDisplay>();
			debugView = canvas.GetElement<DebugView>();

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
			var surface = new SurfaceTriangle(triangle, normal, 0);

			// TODO: Process running into walls while grounded.
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

		// Note that the position given is the ground position.
		private void OnLanding(vec3 p, SurfaceTriangle surface)
		{
			// Project onto the surface. Note that setting ground position *before* the onGround flag, the body's
			// velocity isn't wastefully set twice (since it's forcibly set to zero below).
			surface.Project(p, out vec3 result);
			GroundPosition = result;

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
				if (contacts[i].Owner is IInteractive target && target.IsInteractionEnabled)
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
			var entries = new []
			{
				$"Position: {Position.x:N2}, {Position.y:N2}, {Position.z:N2}",
				$"Old position: {oldPosition.x:N2}, {oldPosition.y:N2}, {oldPosition.z:N2}",
				$"Surface velocity: {SurfaceVelocity.x:N2}, {SurfaceVelocity.y:N2}, {SurfaceVelocity.z:N2}",
				$"Body velocity: {v.x:N2}, {v.y:N2}, {v.z:N2}",
				$"Body gravity: {controllingBody.AffectedByGravity}",
				$"On ground: {onGround}",
				$"Jump enabled: {skillsEnabled[JumpIndex]}",
				$"Jump decelerating: {isJumpDecelerating}",
				$"State: {State}"
			};

			debugView.GetGroup("Player").AddRange(entries);

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
