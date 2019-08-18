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
		private RigidBody body3D;
		private InputBind jumpBindUsed;
		private PlayerData playerData;
		private PlayerControls controls;
		private PlayerController controller;
		private Sword sword;
		private Bow bow;
		
		private bool[] skillsUnlocked;
		private bool[] skillsEnabled;
		private bool jumpedThisFrame;

		public Player() : base(EntityGroups.Player)
		{
			sword = new Sword();
			controls = new PlayerControls();
			playerData = new PlayerData();
			controller = new PlayerController(this, playerData, controls);
			onGround = true;

			int skillCount = Utilities.EnumCount<PlayerSkills>();

			skillsUnlocked = new bool[skillCount];
			skillsEnabled = new bool[skillCount];

			RunAcceleration = Properties.GetFloat("player.run.acceleration");
			RunDeceleration = Properties.GetFloat("player.run.deceleration");
			RunMaxSpeed = Properties.GetFloat("player.run.max.speed");

			Attach(new RunController(this));
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

		// This is used by the player controller.
		public bool[] SkillsEnabled => skillsEnabled;

		public override void Initialize(Scene scene, JToken data)
		{
			Height = Properties.GetFloat("player.height");

			var radius = Properties.GetFloat("player.ground.radius");
			var groundShape = new Circle(radius);

			CreateModel(scene, "Capsule.obj");

			body3D = CreateRigidBody3D(scene, new CylinderShape(Height, radius));
			sensor = CreateSensor(scene, groundShape, SensorUsages.Hitbox | SensorUsages.Interaction, Height);
			CreateSensor(scene, new Point(), SensorUsages.Control, 1, null, -0.75f);

			base.Initialize(scene, data);
		}

		public override void OnCollision(SurfaceTriangle surface, vec3 point, vec3 normal)
		{
		}

		public void Jump()
		{
			skillsEnabled[JumpIndex] = false;
			onGround = false;
			jumpedThisFrame = true;
			
			//controllingBody3D.LinearVelocity = new JVector(v.x, playerData.JumpSpeed, v.y);

			Attach(new AirController(this));
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

		public void Equip(Bow bow)
		{
			this.bow = bow;
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

		protected override void OnLand()
		{
			if (jumpedThisFrame)
			{
				return;
			}

			skillsEnabled[JumpIndex] = skillsUnlocked[JumpIndex];

			base.OnLand();
		}

		public override void Update(float dt)
		{
			body3D.Orientation = JMatrix.Identity;
			jumpedThisFrame = false;

			var velocity = controllingBody3D.LinearVelocity.ToVec3();

			DebugView.Lines = new []
			{
				$"Position: {Position.x}, {Position.y}, {Position.z}",
				$"Velocity: {velocity.x}, {velocity.y}",
				$"On ground: {onGround}",
				$"Jump enabled: {skillsEnabled[JumpIndex]}"
			};

			base.Update(dt);
		}
	}
}
