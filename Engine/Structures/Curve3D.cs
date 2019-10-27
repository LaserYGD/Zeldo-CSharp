using System.Collections.Generic;
using GlmSharp;

namespace Engine.Structures
{
	public class Curve3D
	{
		public Curve3D()
		{
			ControlPoints = new List<vec3>();
		}

		public List<vec3> ControlPoints { get; }

		public vec3[] ComputePoints(int segments)
		{
			vec3[] results = new vec3[segments + 1];

			for (int i = 0; i <= segments; i++)
			{
				float t = 1f / segments * i;
				results[i] = Evaluate(t);
			}

			return results;
		}

		public vec3 Evaluate(float t)
		{
			float u = 1 - t;

			// It's assumed there are at least three control points (or it's not actually a curve).
			vec3 p0 = ControlPoints[0];
			vec3 p1 = ControlPoints[1];
			vec3 p2 = ControlPoints[2];

			// See https://stackoverflow.com/a/5634528/7281613 (and the answer below).
			switch (ControlPoints.Count)
			{
				// Quadratic
				case 3: return (u * u * p0) + (2 * u * t * p1) + (t * t * p2);

				// Cubic
				// x = (1-t)*(1-t)*(1-t)*p0x + 3*(1-t)*(1-t)*t*p1x + 3*(1-t)*t*t*p2x + t*t*t*p3x;
				case 4:
					vec3 p3 = ControlPoints[3];

					return (u * u * u * p0) + (3 * u * u * t * p1) + (3 * u * t * t * p2) + (t * t * t * p3);
			}

			return vec3.Zero;
		}
	}
}
