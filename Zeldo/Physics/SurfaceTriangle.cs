using System;
using Engine;
using Engine.Utility;
using GlmSharp;

namespace Zeldo.Physics
{
	public class SurfaceTriangle
	{
		public static vec2[] FlatPoints { get; private set; }
		public static vec3 Axis { get; private set; }

		// These areused to project points onto the triangle (primarily used to "stick" actors onto a surface while
		// moving).
		private quat projectionQuat;

		// "f" stands for "flat", but using a single character condenses complex calculation lines.
		private vec2 fp0;
		private vec2 fp1;
		private vec2 fp2;

		// For projection purposes, the double area (2 * area) is what you want.
		private float doubleArea;

		public SurfaceTriangle(vec3 p0, vec3 p1, vec3 p2, int material, Windings winding) :
			this(p0, p1, p2,
				Utilities.Normalize(Utilities.Cross(p1 - p0, p2 - p0) * (winding == Windings.Clockwise ? 1 : -1)),
				material)
		{
		}

		public SurfaceTriangle(vec3[] points, vec3 normal, int material) :
			this(points[0], points[1], points[2], normal, material)
		{
		}

		private SurfaceTriangle(vec3 p0, vec3 p1, vec3 p2, vec3 normal, int material)
		{
			Points = new[] { p0, p1, p2 };
			Normal = normal;
			Material = material;

			var angle = Utilities.Angle(Normal, vec3.UnitY);
			var axis = Utilities.Cross(vec3.UnitY, Normal);
			Axis = axis;

			projectionQuat = quat.FromAxisAngle(angle, axis);

			vec2[] flatPoints = new vec2[3];

			for (int i = 0; i < Points.Length; i++)
			{
				flatPoints[i] = (Points[i] * projectionQuat).swizzle.xz;
			}

			fp0 = flatPoints[0];
			fp1 = flatPoints[1];
			fp2 = flatPoints[2];
			FlatPoints = new[] { fp0, fp1, fp2 };

			// See https://stackoverflow.com/a/14382692/7281613.
			doubleArea = -fp1.y * fp2.x + fp0.y * (-fp1.x + fp2.x) + fp0.x * (fp1.y - fp2.y) + fp1.x * fp2.y;

			float theta = Constants.PiOverTwo - Utilities.Angle(new vec3(Normal.x, 0, Normal.z), Normal);

			Slope = (float)Math.Sin(theta);
		}

		public vec3[] Points { get; }
		public vec3 Normal { get; }

		public int Material { get; }

		// This value is used to aid ground movement.
		public float Slope { get; }

		public bool Project(vec3 p, out vec3 result)
		{
			// This is the flat projection.
			vec2 a = (p * projectionQuat).swizzle.xz;

			// See https://stackoverflow.com/a/14382692/7281613.
			float s = 1 / doubleArea * (fp0.y * fp2.x - fp0.x * fp2.y + a.x * (fp2.y - fp0.y) + a.y * (fp0.x - fp2.x));
			float t = 1 / doubleArea * (fp0.x * fp1.y - fp0.y * fp1.x + a.x * (fp0.y - fp1.y) + a.y * (fp1.x - fp0.x));
			float u = 1 - s - t;

			// The projected point is computed regardless (to help account for weird behavior on the edges of
			// triangles, where an actor could theoretically hit a seam and give a false negative). In those cases, the
			// actor continues running past the triangle (which shouldn't be noticeable in practice, since this
			// situation can only occur when you're extremely close to an edge).
			result = Points[1] * s + Points[2] * t + Points[0] * u;

			return !(s < 0) && !(t < 0) && !(u < 0);
		}
	}
}
