using System.Collections.Generic;
using Engine;
using Engine.Core._3D;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Utility;
using GlmSharp;
using Zeldo.Entities.Core;
using Zeldo.Entities.Weapons;
using Zeldo.Interfaces;
using Zeldo.Items;
using Zeldo.Sensors;
using Zeldo.UI;
using Zeldo.UI.Hud;

namespace Zeldo.Entities
{
	public class Player : Entity, IReceiver
	{
		private const int DashIndex = (int)PlayerSkills.Dash;
		private const int GrabIndex = (int)PlayerSkills.Grab;
		private const int JumpIndex = (int)PlayerSkills.Jump;

		private vec3 velocity;
		private Sensor sensor;
		private InputBind jumpBindUsed;
		private PlayerData playerData;
		private PlayerControls controls;
		private Model model;
		private Sword sword;
		private Bow bow;

		private bool onGround;
		private bool[] skillsUnlocked;
		private bool[] skillsEnabled;

		public Player() : base(EntityGroups.Player)
		{
			onGround = true;
			sword = new Sword();
			playerData = JsonUtilities.Deserialize<PlayerData>("PlayerData.json");
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
			CreateModel(scene, "Player.obj");

			base.Initialize(scene);
		}

		public override void Dispose()
		{
			MessageSystem.Unsubscribe(this);
		}

		private void ProcessInput(FullInputData data)
		{
			ProcessAttack(data);
			ProcessJumping(data);
			ProcessRunning(data);
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
				velocity.z = playerData.RunMaxSpeed * (runUp ? -1 : 1);
			}
			else
			{
				velocity.z = 0;
			}
		}

		private void ProcessInteraction(FullInputData data)
		{
			if (!data.Query(controls.Interact, InputStates.PressedThisFrame))
			{
				return;
			}

			foreach (Sensor contact in sensor.Contacts)
			{
				if (contact.Owner is IInteractive target && target.InteractionEnabled)
				{
					target.OnInteract(this);
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
			Position += velocity * dt;

			bow.Position = Position;
			bow.Update(dt);

			//sword.Position = Position;
			//sword.Update(dt);

			DebugView.Lines = new []
			{
				"Grab enabled: " + skillsEnabled[GrabIndex]
			};
		}
	}
}
