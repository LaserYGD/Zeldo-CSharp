using System.Collections.Generic;
using Engine;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Shapes._2D;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Zeldo.Entities.Core;
using Zeldo.Entities.Weapons;
using Zeldo.Interfaces;
using Zeldo.Items;
using Zeldo.Sensors;
using Zeldo.UI;
using Zeldo.UI.Hud;

namespace Zeldo.Entities
{
	public class Player : Actor, IReceiver
	{
		private const int DashIndex = (int)PlayerSkills.Dash;
		private const int GrabIndex = (int)PlayerSkills.Grab;
		private const int JumpIndex = (int)PlayerSkills.Jump;
		
		private Sensor sensor;
		private RigidBody body3D;
		private InputBind jumpBindUsed;
		private PlayerData playerData;
		private PlayerControls controls;
		private Sword sword;
		private Bow bow;
		
		private bool[] skillsUnlocked;
		private bool[] skillsEnabled;

		public Player() : base(EntityGroups.Player)
		{
			onGround = true;
			sword = new Sword();
			playerData = new PlayerData();
			controls = new PlayerControls();

			int skillCount = Utilities.EnumCount<PlayerSkills>();

			skillsUnlocked = new bool[skillCount];
			skillsEnabled = new bool[skillCount];

			MessageSystem.Subscribe(this, CoreMessageTypes.Input, (messageType, data, dt) =>
			{
				ProcessInput((FullInputData)data);
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }
		public PlayerHealthDisplay HealthDisplay { get; set; }
		public PlayerManaDisplay ManaDisplay { get; set; }
		public PlayerDebugView DebugView { get; set; }

		// The player owns their own inventory.
		public Inventory Inventory { get; }

		public override void Initialize(Scene scene)
		{
			Height = Properties.GetFloat("player.height");

			var radius = Properties.GetFloat("player.ground.radius");
			var groundShape = new Circle(radius);

			CreateModel(scene, "Player.obj");

			body3D = CreateRigidBody3D(scene, new CylinderShape(Height, radius));
			groundBody = CreateGroundBody(scene, groundShape);
			StairPosition = new vec2(0, 5.5f);
			sensor = CreateSensor(scene, groundShape);

			base.Initialize(scene);
		}

		public override void Dispose()
		{
			MessageSystem.Unsubscribe(this);
		}

		private void ProcessInput(FullInputData data)
		{
			//ProcessAttack(data);
			ProcessJumping(data);
			ProcessRunning(data);
			ProcessInteraction(data);
		}

		private void ProcessAttack(FullInputData data)
		{
			if (data.Query(controls.Attack, InputStates.PressedThisFrame, out InputBind bindUsed))
			{
				vec2 direction = vec2.Zero;

				switch (bindUsed.InputType)
				{
					case InputTypes.Keyboard:
						break;

					case InputTypes.Mouse:
						MouseData mouseData = (MouseData)data.GetData(InputTypes.Mouse);

						vec4 projected = Scene.Camera.ViewProjection * new vec4(Position, 1);
						vec2 halfWindow = Resolution.WindowDimensions / 2;
						vec2 screenPosition = projected.swizzle.xy * halfWindow;

						screenPosition.y *= -1;
						screenPosition += halfWindow;
						direction = (mouseData.Location - screenPosition).Normalized;

						break;
				}

				float angle = Utilities.Angle(direction);

				//sword.Attack(direction);
				bow.PrimaryAttack(direction, angle);
			}
		}

		private void ProcessJumping(FullInputData data)
		{
			if (!skillsEnabled[JumpIndex])
			{
				return;
			}
		}

		private void ProcessRunning(FullInputData data)
		{
			bool runLeft = data.Query(controls.RunLeft, InputStates.Held);
			bool runRight = data.Query(controls.RunRight, InputStates.Held);
			bool runUp = data.Query(controls.RunUp, InputStates.Held);
			bool runDown = data.Query(controls.RunDown, InputStates.Held);

			vec2 velocity = groundBody.Velocity;

			if (runLeft ^ runRight)
			{
				velocity.x = playerData.RunMaxSpeed * (runLeft ? -1 : 1);
			}
			else
			{
				velocity.x = 0;
			}

			if (runUp ^ runDown)
			{
				velocity.y = playerData.RunMaxSpeed * (runUp ? -1 : 1);
			}
			else
			{
				velocity.y = 0;
			}

			groundBody.Velocity = velocity;
		}

		private void ProcessInteraction(FullInputData data)
		{
			if (!data.Query(controls.Interact, InputStates.PressedThisFrame))
			{
				return;
			}

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

		public override void Update(float dt)
		{
			body3D.Orientation = JMatrix.Identity;

			var velocity = groundBody.Velocity;

			DebugView.Lines = new []
			{
				$"Position: {Position.x}, {Position.y}, {Position.z}",
				$"Stair position: {StairPosition.x}, {StairPosition.y}",
				$"Velocity: {velocity.x}, {velocity.y}"
			};

			base.Update(dt);
		}
	}
}
