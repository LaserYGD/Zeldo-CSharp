using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core;
using Engine.Entities;
using Engine.Graphics;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Shapes._3D;
using Engine.View;
using GlmSharp;
using Zeldo.Entities.Weapons;
using Zeldo.UI.Hud;

namespace Zeldo.Entities
{
	public class Player : Entity3D, IReceiver
	{
		private vec3 velocity;
		private Sword sword;
		private PlayerControls controls;

		public Player()
		{
			Box = new Box(0.6f, 1.8f, 0.6f);
			sword = new Sword();
			controls = new PlayerControls();

			MessageSystem.Subscribe(this, CoreMessageTypes.Input, (messageType, data, dt) =>
			{
				ProcessInput((FullInputData)data);
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }
		public PlayerHealthDisplay HealthDisplay { get; set; }
		public PlayerManaDisplay ManaDisplay { get; set; }

		public Box Box { get; }

		private void ProcessInput(FullInputData data)
		{
			ProcessAttack(data);
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

						direction = (mouseData.Location - Position.swizzle.xz).Normalized;

						break;
				}

				sword.Attack(direction);
			}
		}

		private void ProcessRunning(FullInputData data)
		{
			const int Speed = 3;

			bool runLeft = data.Query(controls.RunLeft, InputStates.Held);
			bool runRight = data.Query(controls.RunRight, InputStates.Held);
			bool runUp = data.Query(controls.RunUp, InputStates.Held);
			bool runDown = data.Query(controls.RunDown, InputStates.Held);

			if (runLeft ^ runRight)
			{
				velocity.x = Speed * (runLeft ? -1 : 1);
			}
			else
			{
				velocity.x = 0;
			}

			if (runUp ^ runDown)
			{
				velocity.z = Speed * (runUp ? -1 : 1);
			}
			else
			{
				velocity.z = 0;
			}
		}

		public override void Update(float dt)
		{
			Position += velocity * dt;
			Box.Position = Position;
		}
	}
}
