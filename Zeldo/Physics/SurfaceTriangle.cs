using System;
using System.Linq;
using Engine;
using Engine.Physics;
using Engine.Utility;
using GlmSharp;
using Jitter.LinearMath;

namespace Zeldo.Physics
{
	public class SurfaceTriangle
	{
		public static SurfaceTypes ComputeSurfaceType(JVector[] triangle, WindingTypes winding)
		{
			var p0 = triangle[0];
			var p1 = triangle[1];
			var p2 = triangle[2];

			return ComputeSurfaceType(Utilities.ComputeNormal(p0, p1, p2, winding).ToVec3(), out _);
		}

		public static SurfaceTypes ComputeSurfaceType(vec3 normal)
		{
			return ComputeSurfaceType(normal, out _);
		}

		private static SurfaceTypes ComputeSurfaceType(vec3 normal, out float theta)
		{
			// This is the tilt angle from a perfectly flat floor.
			theta = ComputeTheta(normal);

			// The wall threshold is defined as an angle range from a vertical wall (theta 90 degrees).
			return Math.Abs(Constants.PiOverTwo - theta) <= PhysicsConstants.WallThreshold
				? SurfaceTypes.Wall
				: (normal.y < 0 ? SurfaceTypes.Ceiling : SurfaceTypes.Floor);
		}

		private static float ComputeTheta(vec3 normal)
		{
			return normal == vec3.UnitY
				? 0
				: Constants.PiOverTwo - Utilities.Angle(new vec3(normal.x, 0, normal.z), normal);
		}
		
		public static float ComputeForgiveness(vec3 p, SurfaceTriangle surface)
		{
			// To compute the shortest distance to an edge of the triangle, points are rotated to a flat plane first
			// (using the surface normal).
			var q = Utilities.Orientation(surface.Normal, vec3.UnitY);
			var flatP = (q * p).swizzle.xz;
			var flatPoints = surface.Points.Select(v => (q * v).swizzle.xz).ToArray();
			var d = float.MaxValue;

			for (int i = 0; i < flatPoints.Length; i++)
			{
				var p1 = flatPoints[i];
				var p2 = flatPoints[(i + 1) % 3];

				d = Math.Min(d, Utilities.DistanceToLine(flatP, p1, p2));
			}

			return d;
		}

		// These are used to project points onto the triangle (primarily used to "stick" actors onto a surface while
		// moving).
		private quat flatProjection;

		// "f" stands for "flat", but using a single character condenses complex calculation lines.
		private vec2 fp0;
		private vec2 fp1;
		private vec2 fp2;

		// For projection purposes, the double area (2 * area) is what you want.
		private float doubleArea;

		public SurfaceTriangle(vec3 p0, vec3 p1, vec3 p2, WindingTypes winding, int material,
			bool shouldComputeFlatNormal = false) :
			this(p0, p1, p2, Utilities.ComputeNormal(p0, p1, p2, winding), material, null, shouldComputeFlatNormal)
		{
		}

		public SurfaceTriangle(vec3[] points, vec3 normal, int material, SurfaceTypes? surfaceType = null,
			bool shouldComputeFlatNormal = false) :
			this(points[0], points[1], points[2], normal, material, surfaceType, shouldComputeFlatNormal)
		{
		}

		public SurfaceTriangle(JVector[] points, vec3 normal, int material, SurfaceTypes? surfaceType = null,
			bool shouldComputeFlatNormal = false) :
			this(points[0].ToVec3(), points[1].ToVec3(), points[2].ToVec3(), normal, material, surfaceType,
				shouldComputeFlatNormal)
		{
		}

		private SurfaceTriangle(vec3 p0, vec3 p1, vec3 p2, vec3 normal, int material, SurfaceTypes? surfaceType,
			bool shouldComputeFlatNormal)
		{
			Points = new[] { p0, p1, p2 };
			Normal = normal;
			Material = material;
			flatProjection = Utilities.ComputeFlatProjection(normal);

			vec2[] flatPoints = new vec2[3];

			for (int i = 0; i < Points.Length; i++)
			{
				flatPoints[i] = (Points[i] * flatProjection).swizzle.xz;
			}

			fp0 = flatPoints[0];
			fp1 = flatPoints[1];
			fp2 = flatPoints[2];

			// See https://stackoverflow.com/a/14382692/7281613.
			doubleArea = -fp1.y * fp2.x + fp0.y * (-fp1.x + fp2.x) + fp0.x * (fp1.y - fp2.y) + fp1.x * fp2.y;

			// In some cases, surface type is pre-computed outside this constructor. Passing it in saves the
			// calculations here.
			float theta;

			if (surfaceType.HasValue)
			{
				SurfaceType = surfaceType.Value;
				theta = ComputeTheta(normal);
			}
			else
			{
				SurfaceType = ComputeSurfaceType(normal, out theta);
			}
			
			Slope = (float)Math.Sin(theta);

			// Downward-facing triangles are given a negative slope.
			if (normal.y < 0)
			{
				Slope *= -1;
			}

			// The flat normal is used when processing wall control for the player. By definition, this only applies to
			// walls.
			if (SurfaceType == SurfaceTypes.Wall && shouldComputeFlatNormal)
			{
				FlatNormal = Utilities.Normalize(normal.x, 0, normal.z);
			}
		}

		public vec3[] Points { get; }
		public vec3 Normal { get; }
		public vec3 FlatNormal { get; }

		public SurfaceTypes SurfaceType { get; }

		public int Material { get; }

		// This value (used primarily to aid ground movement) is between 0 and 1, with 0 representing a completely flat
		// surface and 1 representing a completely vertical one.
		// TODO: Can (or should) this value become negative for over-hanging triangles whose normal points downward?
		public float Slope { get; }

		public bool Project(vec3 p, out vec3 result)
		{
			// This is the flat projection.
			vec2 a = (p * flatProjection).swizzle.xz;

			// See https://stackoverflow.com/a/14382692/7281613.
			float s = 1 / doubleArea * (fp0.y * fp2.x - fp0.x * fp2.y + a.x * (fp2.y - fp0.y) + a.y * (fp0.x - fp2.x));
			float t = 1 / doubleArea * (fp0.x * fp1.y - fp0.y * fp1.x + a.x * (fp0.y - fp1.y) + a.y * (fp1.x - fp0.x));
			float u = 1 - s - t;

			// The projected point is computed regardless (to help account for weird behavior on the edges of
			// triangles, where an actor could theoretically hit a seam and give a false negative). In those cases, the
			// actor continues running past the triangle (which shouldn't be noticeable in practice, since this
			// situation can only occur when you're extremely close to an edge).
			result = Points[1] * s + Points[2] * t + Points[0] * u;

			return s >= 0 && t >= 0 && u >= 0;
		}

		public bool IsSame(JVector[] triangle)
		{
			return IsSame(triangle.Select(t => t.ToVec3()).ToArray());
		}

		public bool IsSame(vec3[] triangle)
		{
			const float Epsilon = 0.001f;

			for (int i = 0; i < 3; i++)
			{
				if (Utilities.DistanceSquared(triangle[i], Points[i]) > Epsilon)
				{
					return false;
				}
			}

			return true;
		}
	}
}
