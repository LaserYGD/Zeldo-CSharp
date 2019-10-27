using System;
using System.Linq;
using Engine.Interfaces;
using Engine.Utility;
using GlmSharp;

namespace Engine.Physics.Verlet
{
	public class VerletRope2D : IDynamic
	{
		private const int Iterations = 8;

		private float k;
		private float damping;
		private float gravity;
		private float segmentLength;

		private VerletPoint2D[] points;

		public VerletRope2D(vec2[] points, float segmentLength, float k, float damping, float gravity)
		{
			this.k = k;
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
				/*
				var distances = new float[points.Length - 1];

				// TODO: Do all of these distances need to be computed each iteration?
				for (int j = 0; j < points.Length - 1; j++)
				{
					distances[j] = Utilities.Distance(points[j].Position, points[j + 1].Position);
				}
				*/

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

					/*
					if (j == 1)
					{
						p -= results[0] * 2;
					}
					else if (j == points.Length - 2)
					{
						p -= results.Last() * 2;
					}
					else
					{
						p += results[j] - results[j - 1];
					}
					*/

					p += results[j] - results[j - 1];
					point.Position = p;
				}

				/*
				for (int j = 1; j < points.Length - 1; j++)
				{
					var d1 = distances[j - 1];
					var d2 = distances[j];

					var point = points[j];
					var p0 = points[j - 1].Position;
					var p1 = point.Position;
					var p2 = points[j + 1].Position;
					var squared = segmentLength * segmentLength;

					bool b1 = Utilities.DistanceSquared(p0, p1) > squared;
					bool b2 = Utilities.DistanceSquared(p1, p2) > squared;

					vec2 r = vec2.Zero;

					// If only one constraint is breached, the point is pulled fully in that direction.
					if (b1 ^ b2)
					{
						var d = b1 ? d1 : d2;
						var v = (b1 ? p0 : p2) - p1;

						r = v / d * (d - segmentLength);
					}
					// If both constraints are breached, the point is pulled in both directions.
					else if (b1)
					{
						var v1 = (p0 - p1) / d1 * (d1 - segmentLength);
						var v2 = (p2 - p1) / d2 * (d2 - segmentLength);

						// Previously, I used projection to resolve the two vectors. In practice, that made the rope
						// simulation pretty unstable.
						r = (v1 + v2) / 2;
					}

					results[j - 1] += r;
				}

				for (int j = 1; j < points.Length - 1; j++)
				{
					//points[j].Position += results[j - 1];
				}
				*/
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
