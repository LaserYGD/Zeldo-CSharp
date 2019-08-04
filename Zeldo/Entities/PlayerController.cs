using System.Collections.Generic;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Zeldo.Interfaces;

namespace Zeldo.Entities
{
	public class PlayerController : IReceiver
	{
		private const int DashIndex = (int)PlayerSkills.Dash;
		private const int GrabIndex = (int)PlayerSkills.Grab;
		private const int JumpIndex = (int)PlayerSkills.Jump;

		private Player player;
		private PlayerData playerData;
		private PlayerControls controls;

		public PlayerController(Player player, PlayerData playerData, PlayerControls controls)
		{
			this.player = player;
			this.playerData = playerData;
			this.controls = controls;

			MessageSystem.Subscribe(this, CoreMessageTypes.Input, (messageType, data, dt) =>
			{
				ProcessInput((FullInputData)data);
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }

		public void Dispose()
		{
		}

		private void ProcessInput(FullInputData data)
		{
			ProcessRunning(data);
			ProcessJumping(data);
			ProcessInteraction(data);
		}

		/*
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
		*/

		private void ProcessRunning(FullInputData data)
		{
		}

		private void ProcessJumping(FullInputData data)
		{
			if (player.SkillsEnabled[JumpIndex] && data.Query(controls.Jump, InputStates.PressedThisFrame))
			{
				player.Jump();
			}
		}

		private void ProcessInteraction(FullInputData data)
		{
			if (data.Query(controls.Interact, InputStates.PressedThisFrame))
			{
				player.Interact();
			}
		}
	}
}
