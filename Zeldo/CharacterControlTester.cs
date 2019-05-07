using System;
using System.Collections.Generic;
using Engine;
using Engine.Core;
using Engine.Graphics._2D;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Messaging;
using Engine.Shapes._2D;
using Engine.Utility;
using GlmSharp;
using static Engine.GLFW;

namespace Zeldo
{
	public class CharacterControlTester : IReceiver, IDynamic, IRenderable2D
	{
		private const int MaxSpeed = 300;
		private const int Acceleration = 2000;
		private const int Deceleration = 1200;

		private Circle playerCircle;
		private List<Circle> staticCircles;
		private List<Line> staticLines;
		private vec2 playerVelocity;

		public CharacterControlTester()
		{
			const int Padding = 10;
			const int WallX = 800;
			const int WallY = 150;

			playerCircle = new Circle(30);
			playerCircle.Position = Resolution.WindowDimensions / 2;

			staticCircles = new List<Circle>();
			staticCircles.Add(new Circle(100) { Position = new vec2(200) });
			staticCircles.Add(new Circle(80) { Position = new vec2(290, 310) });
			staticCircles.Add(new Circle(250) { Position = new vec2(950, 600) });

			vec2[] linePoints =
			{
				new vec2(Padding),
				new vec2(WallX, Padding),
				new vec2(WallX, 150),
				new vec2(Resolution.WindowWidth - Padding, 150),
				new vec2(Resolution.WindowWidth - Padding, Resolution.WindowHeight - Padding),
				new vec2(Padding, Resolution.WindowHeight - Padding)
			};

			staticLines = new List<Line>();

			for (int i = 0; i < linePoints.Length; i++)
			{
				vec2 p1 = linePoints[i];
				vec2 p2 = i == linePoints.Length - 1 ? linePoints[0] : linePoints[i + 1];

				staticLines.Add(new Line(p1, p2));
			}

			MessageSystem.Subscribe(this, CoreMessageTypes.Keyboard, (messageType, data, dt) =>
			{
				ProcessKeyboard((KeyboardData)data, dt);
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }

		private void ProcessKeyboard(KeyboardData data, float dt)
		{
			bool up = data.Query(GLFW_KEY_W, InputStates.Held);
			bool down = data.Query(GLFW_KEY_S, InputStates.Held);
			bool left = data.Query(GLFW_KEY_A, InputStates.Held);
			bool right = data.Query(GLFW_KEY_D, InputStates.Held);

			vec2 accelerationDirection = vec2.Zero;

			if (left ^ right)
			{
				accelerationDirection.x = left ? -1 : 1;
			}

			if (up ^ down)
			{
				accelerationDirection.y = up ? -1 : 1;
			}

			if (accelerationDirection != vec2.Zero)
			{
				playerVelocity += accelerationDirection * Acceleration * dt;

				if (Utilities.LengthSquared(playerVelocity) > MaxSpeed * MaxSpeed)
				{
					playerVelocity = Utilities.Normalize(playerVelocity) * MaxSpeed;
				}
			}
			else if (playerVelocity != vec2.Zero)
			{
				int previousSign = Math.Sign(playerVelocity.x != 0 ? playerVelocity.x : playerVelocity.y);

				playerVelocity -= Utilities.Normalize(playerVelocity) * Deceleration * dt;

				int newSign = Math.Sign(playerVelocity.x != 0 ? playerVelocity.x : playerVelocity.y);

				if (newSign != previousSign)
				{
					playerVelocity = vec2.Zero;
				}
			}
		}

		public void Update(float dt)
		{
			playerCircle.Position += playerVelocity * dt;

			List<vec2> correctionVectors = new List<vec2>();

			staticCircles.ForEach(c =>
			{
				if (ResolveStaticCircleCollision(c, out vec2 v))
				{
					correctionVectors.Add(v);
				}
			});

			staticLines.ForEach(l =>
			{
				if (ResolveStaticLineCollision(l, out vec2 v))
				{
					correctionVectors.Add(v);
				}
			});

			int count = correctionVectors.Count;

			if (count == 0)
			{
				return;
			}

			vec2 finalCorrection = vec2.Zero;
			
			if (count == 1)
			{
				finalCorrection = correctionVectors[0];
			}
			else
			{
				for (int i = 0; i < count; i++)
				{
					vec2 v = correctionVectors[i];

					finalCorrection += v;

					for (int j = i + 1; j < count; j++)
					{
						correctionVectors[i] = Utilities.Project(v, correctionVectors[i]);
					}
				}
			}

			playerCircle.Position += finalCorrection;

			/*
			vec2 correction = -Utilities.Normalize(finalCorrection);
			vec2 vNormalized = Utilities.Normalize(playerVelocity);

			playerVelocity *= 1 - vec2.Dot(correction, vNormalized);
			*/
		}

		private bool ResolveStaticCircleCollision(Circle staticCircle, out vec2 correctionVector)
		{
			float r1 = playerCircle.Radius;
			float r2 = staticCircle.Radius;
			float sum = r1 + r2;
			float distanceSquared = Utilities.DistanceSquared(playerCircle.Position, staticCircle.Position);

			if (distanceSquared < sum * sum)
			{
				float distance = (float)Math.Sqrt(distanceSquared);
				float penetration = r2 - distance;

				vec2 vector = (playerCircle.Position - staticCircle.Position) / distance;

				correctionVector = vector * (penetration + r1);

				return true;
			}

			correctionVector = vec2.Zero;

			return false;
		}

		private bool ResolveStaticLineCollision(Line line, out vec2 correctionVector)
		{
			float d = Utilities.DistanceSquaredToLine(playerCircle.Position, line);
			float r = playerCircle.Radius;

			if (d < r * r)
			{
				float penetration = r - (float)Math.Sqrt(d);

				// This assumes that all static lines will be oriented such that the right-hand vector faces back into
				// the stage (in order to be used a correction vector).
				vec2 l = line.P2 - line.P1;
				vec2 outVector = new vec2(-l.y, l.x);

				correctionVector = Utilities.Normalize(outVector) * penetration;

				return true;
			}

			correctionVector = vec2.Zero;

			return false;
		}

		public void Draw(SpriteBatch sb)
		{
			const int Segments = 25;

			sb.Draw(playerCircle, Segments, Color.Cyan);

			foreach (Circle circle in staticCircles)
			{
				sb.Draw(circle, Segments, Color.Green);
			}

			foreach (Line line in staticLines)
			{
				sb.Draw(line, Color.Yellow);
			}
		}
	}
}
