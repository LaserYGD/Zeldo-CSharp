using System;
using Engine.Interfaces;
using Engine.Utility;
using GlmSharp;

namespace Engine.Physics.Verlet
{
	public class VerletRope2D : IDynamic
	{
		private const int Iterations = 8;
		
		private float damping;
		private float gravity;
		private float segmentLength;

		private VerletPoint2D[] points;

		public VerletRope2D(vec2[] points, float segmentLength, float damping, float gravity)
		{
			this.damping = damping;
			this.gravity = gravity;
			this.segmentLength = segmentLength;
			this.points = new VerletPoint2D[points.Length];

			for (int i = 0; i < points.Length; i++)
			{
				this.points[i] = new VerletPoint2D(points[i]);
			}
		}

		public VerletPoint2D[] Points => points;

		// TODO: Verify that ropes behave the same at varying framerates.
		public void Update(float dt)
		{
			// TODO: This assumes both endpoints are fixed. Generalize for any number of fixed points if needed.
			for (int i = 1; i < points.Length - 1; i++)
			{
				var point = points[i];
				var p = point.Position;
				var temp = p;

				p.y += gravity * dt;
				p += (p - point.OldPosition) * damping;
				point.Position = p;
				point.OldPosition = temp;
			}

			SolveConstraints();
		}

		private void SolveConstraints()
		{
			for (int i = 0; i < Iterations; i++)
			{
				// There's one constraint vector per segment.
				var results = new vec2[points.Length - 1];

				for (int j = 0; j < points.Length - 1; j++)
				{
					var p1 = points[j].Position;
					var p2 = points[j + 1].Position;
					var squared = Utilities.DistanceSquared(p1, p2);

					if (squared > segmentLength * segmentLength)
					{
						var d = (float)Math.Sqrt(squared);
						var delta = d - segmentLength;

						results[j] = (p2 - p1) / d * delta / 2;
					}
				}

				for (int j = 1; j < points.Length - 1; j++)
				{
					var point = points[j];
					var p = point.Position;

					p += results[j] - results[j - 1];
					point.Position = p;
				}
			}

			// Compute rotations (right-handed).
			for (int i = 1; i < points.Length - 1; i++)
			{
				var angle = Utilities.Angle(points[i - 1].Position, points[i + 1].Position);

				points[i].Rotation = angle - Constants.PiOverTwo;
			}
		}
	}
}
