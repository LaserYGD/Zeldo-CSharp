using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using Engine.Core._2D;
using Engine.Graphics;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Messaging;
using Engine.Timing;
using GlmSharp;
using static Engine.GLFW;

namespace Zeldo
{
	public class JumpTester : IDynamic, IRenderable2D, IReceiver
	{
		private const float JumpHeight = 140;
		private const float JumpDuration = 0.45f;
		private const float FallDuration = 0.35f;

		private Sprite[] sprites;
		
		private bool onGround;
		private bool falling;

		private Timer timer;

		public JumpTester()
		{
			sprites = new Sprite[4];

			for (int i = 0; i < sprites.Length; i++)
			{
				sprites[i] = new Sprite("Link.png", Alignments.Bottom)
				{
					Position = new vec2(160 + 160 * i, 600)
				};
			}

			timer = new SingleTimer(time =>
			{
				if (falling)
				{
					timer.Elapsed = 0;
					timer.Duration = JumpDuration;
					//timer.Paused = true;
					//onGround = true;
					falling = false;

					foreach (Sprite sprite in sprites)
					{
						sprite.Y = Resolution.WindowHeight;
					}

					return;
				}

				timer.Elapsed = time;
				timer.Duration = FallDuration;
				falling = true;
			},
			JumpDuration);

			timer.Tick = progress =>
			{
				if (falling)
				{
					float quadratic = QuadraticIn(progress);
					float cubic = CubicIn(progress);

					foreach (Sprite sprite in sprites)
					{
						sprite.Y = Resolution.WindowHeight - JumpHeight + JumpHeight * quadratic;
					}

					return;
				}

				float[] tArray =
				{
					QuadraticOut(progress),
					CubicOut(progress),
					QuarticOut(progress),
					QuinticOut(progress)
				};

				for (int i = 0; i < sprites.Length; i++)
				{
					sprites[i].Y = Resolution.WindowHeight - JumpHeight * tArray[i];
				}
			};

			//timer.Paused = true;
			//onGround = true;

			MessageSystem.Subscribe(this, CoreMessageTypes.Keyboard, (messageType, data, dt) =>
			{
				ProcessKeyboard((KeyboardData)data);
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }

		private float QuadraticIn(float p)
		{
			return p * p;
		}

		private float QuadraticOut(float p)
		{
			return -(p * (p - 2));
		}

		private float CubicIn(float p)
		{
			return p * p * p;
		}
		
		private float CubicOut(float p)
		{
			float f = p - 1;

			return f * f * f + 1;
		}

		private float QuarticOut(float p)
		{
			float f = p - 1;

			return f * f * f * (1 - p) + 1;
		}

		private float QuinticOut(float p)
		{
			float f = p - 1;

			return f * f * f * f * f + 1;
		}

		private void ProcessKeyboard(KeyboardData data)
		{
			if (data.Query(GLFW_KEY_SPACE, InputStates.PressedThisFrame))
			{
				timer.Paused = false;
				onGround = false;
			}
		}

		public void Update(float dt)
		{
			if (onGround)
			{
				return;
			}
			
			timer.Update(dt);

			/*
			float y = sprites[0].Position.y;

			if (y >= Resolution.WindowHeight)
			{
				y = Resolution.WindowHeight;
				velocity = 0;
				onGround = true;
			}

			if (!onGround)
			{
				y += velocity * dt;
			}

			foreach (Sprite sprite in sprites)
			{
				vec2 p = sprite.Position;
				p.y = y;
				//sprite.Position = p;
			}

			if (!onGround)
			{
				velocity += gravity * dt;
			}
			*/
		}

		public void Draw(SpriteBatch sb)
		{
			foreach (Sprite sprite in sprites)
			{
				sprite.Draw(sb);
			}
		}
	}
}
