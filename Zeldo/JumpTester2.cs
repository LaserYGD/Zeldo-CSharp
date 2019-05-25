using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using Engine.Core._2D;
using Engine.Graphics;
using Engine.Graphics._2D;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Messaging;
using GlmSharp;
using static Engine.GLFW;

namespace Zeldo
{
	public class JumpTester2 : IDynamic, IRenderable2D, IReceiver
	{
		private const int DefaultJumpSpeed = 1600;
		private const int DefaultGravityReset = 10000;
		private const int DefaultGravityLimit = 1000;
		private const int DefaultGravityAcceleration = 20000;

		private Sprite sprite;
		private SpriteText[] textArray;
		private DateTime jumpTime;

		private float velocity;
		private float jumpSpeed;
		private float gravity;
		private float gravityReset;
		private float gravityLimit;
		private float gravityAcceleration;

		private bool onGround;

		private int jumpDuration;

		public JumpTester2()
		{
			sprite = new Sprite("Link.png", null, Alignments.Bottom);
			sprite.Position = new vec2(400, 600);

			textArray = new SpriteText[7];

			for (int i = 0; i < textArray.Length; i++)
			{
				textArray[i] = new SpriteText("Default");
				textArray[i].Position = new vec2(20, 20 * (i + 1));
			}

			jumpSpeed = DefaultJumpSpeed;
			gravity = DefaultGravityReset;
			gravityReset = gravity;
			gravityLimit = DefaultGravityLimit;
			gravityAcceleration = DefaultGravityAcceleration;
			onGround = true;

			MessageSystem.Subscribe(this, CoreMessageTypes.Keyboard, (messageType, data, dt) =>
			{
				ProcessKeyboard((KeyboardData)data);
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }

		public void Dispose()
		{
			MessageSystem.Unsubscribe(this);
		}

		private void ProcessKeyboard(KeyboardData data)
		{
			if (!onGround)
			{
				return;
			}

			if (data.Query(GLFW_KEY_SPACE, InputStates.PressedThisFrame))
			{
				velocity = -jumpSpeed;
				onGround = false;
				jumpTime = DateTime.Now;

				return;
			}

			bool up = data.Query(GLFW_KEY_UP, InputStates.PressedThisFrame);
			bool down = data.Query(GLFW_KEY_DOWN, InputStates.PressedThisFrame);
			bool shift = data.Query(GLFW_KEY_LEFT_SHIFT, InputStates.Held);

			if (!(up ^ down))
			{
				return;
			}

			float[] values =
			{
				jumpSpeed,
				-1,
				-1,
				-1,
				gravityReset,
				gravityLimit,
				gravityAcceleration
			};

			int delta = shift ? 1000 : 100;

			for (int i = 0; i < values.Length; i++)
			{
				if (i >= 1 && i <= 3)
				{
					continue;
				}

				if (data.Query(GLFW_KEY_1 + i, InputStates.Held))
				{
					values[i] += delta * (up ? 1 : -1);
					values[i] = Math.Max(values[i], 0);
				}
			}
			
			jumpSpeed = values[0];
			gravityReset = values[4];
			gravity = gravityReset;
			gravityLimit = values[5];
			gravityAcceleration = values[6];
		}

		public void Update(float dt)
		{
			string[] strings =
			{
				"Jump speed: " + jumpSpeed,
				"Jump duration: " + jumpDuration,
				"Velocity: " + velocity,
				"Gravity: " + gravity,
				"Gravity reset: " + gravityReset,
				"Gravity limit: " + gravityLimit,
				"Gravity acceleration: " + gravityAcceleration
			};

			for (int i = 0; i < textArray.Length; i++)
			{
				textArray[i].Value = $"{i + 1}. {strings[i]}";
			}

			if (onGround)
			{
				return;
			}

			float y = sprite.Y + velocity * dt;

			if (y >= Resolution.WindowHeight)
			{
				y = Resolution.WindowHeight;
				onGround = true;
				velocity = 0;
				gravity = gravityReset;
				jumpDuration = (DateTime.Now - jumpTime).Milliseconds;
			}

			sprite.Y = y;

			if (!onGround)
			{
				if (velocity > 0)
				{
					gravity += gravityAcceleration * dt;
				}

				float previousVelocity = velocity;

				velocity += gravity * dt;

				if (velocity >= 0 && previousVelocity < 0)
				{
					gravity = gravityLimit;
				}
			}
		}

		public void Draw(SpriteBatch sb)
		{
			foreach (SpriteText text in textArray)
			{
				text.Draw(sb);
			}

			sprite.Draw(sb);
		}
	}
}
