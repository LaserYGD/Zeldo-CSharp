using Engine.Utility;
using GlmSharp;

namespace Engine.Physics
{
	public class SurfaceTriangle
	{
		public SurfaceTriangle(vec3 p0, vec3 p1, vec3 p2, int material, Windings winding)
		{
			Points = new [] { p0, p1, p2 };
			Normal = Utilities.Normalize(Utilities.Cross(p1 - p0, p2 - p0) * (winding == Windings.Clockwise ? 1 : -1));
			Material = material;
		}

		public SurfaceTriangle(vec3[] points, vec3 normal, int material)
		{
			Points = points;
			Normal = normal;
			Material = material;
		}

		public vec3[] Points { get; }
		public vec3 Normal { get; }

		public int Material { get; }
	}
}
