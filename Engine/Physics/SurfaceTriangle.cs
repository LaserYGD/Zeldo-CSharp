using GlmSharp;

namespace Engine.Physics
{
	public class SurfaceTriangle
	{
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
