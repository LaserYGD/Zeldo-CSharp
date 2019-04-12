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
using Zeldo.UI.Hud;

namespace Zeldo.Entities
{
	public class Player : Entity3D, IReceiver
	{
		private vec3 velocity;
		private Box box;
		private PlayerControls controls;
		private PrimitiveRenderer3D primitives;

		public Player()
		{
			box = new Box(0.6f, 1.8f, 0.6f);
			controls = new PlayerControls();
			primitives = new PrimitiveRenderer3D();

			MessageSystem.Subscribe(this, CoreMessageTypes.Input, (messageType, data, dt) =>
			{
				ProcessInput((FullInputData)data, dt);
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }
		public PlayerHealthDisplay HealthDisplay { get; set; }
		public PlayerManaDisplay ManaDisplay { get; set; }

		private void ProcessInput(FullInputData data, float dt)
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
			box.Position = Position;
		}

		public override void Draw(Camera3D camera)
		{
			primitives.Draw(box, Color.White);
			primitives.Flush(camera);
		}
	}
}
