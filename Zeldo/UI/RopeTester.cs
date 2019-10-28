using System;
using System.Collections.Generic;
using Engine.Core;
using Engine.Graphics._2D;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Physics.Verlet;
using Engine.UI;
using Engine.Utility;
using GlmSharp;
using static Engine.GLFW;

namespace Zeldo.UI
{
	public class RopeTester : CanvasElement, IReceiver
	{
		private VerletRope2D rope;

		public RopeTester()
		{
			const float Length = 36;
			const float Damping = 0.95f;
			const float Gravity = 20;

			vec2[] points = new vec2[18];

			for (int i = 0; i < points.Length; i++)
			{
				points[i] = new vec2(60, 120) + new vec2(Length * i * 1.25f, 0);
			}

			rope = new VerletRope2D(points, Length, Damping, Gravity);

			MessageSystem.Subscribe(this, CoreMessageTypes.Mouse, (messageType, data, dt) =>
			{
				var mouse = (MouseData)data;

				if (mouse.Query(GLFW_MOUSE_BUTTON_LEFT, InputStates.PressedThisFrame))
				{
					var array = rope.Points;
					var index = new Random().Next(array.Length - 2) + 1;

					array[index].Position += new vec2(0, 20);
				}
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }

		public override void Update(float dt)
		{
			rope.Update(dt);
		}

		public override void Draw(SpriteBatch sb)
		{
			const int NormalLength = 20;
			
			var points = rope.Points;

			// Draw segments.
			for (int i = 0; i < points.Length - 1; i++)
			{
				sb.DrawLine(points[i].Position, points[i + 1].Position, Color.White);
			}

			// Draw rotations.
			for (int i = 1; i < points.Length - 1; i++)
			{
				var point = points[i];
				var p = point.Position;

				sb.DrawLine(p, p + Utilities.Rotate(vec2.UnitX, point.Rotation) * NormalLength, Color.Cyan);
			}
		}
	}
}
