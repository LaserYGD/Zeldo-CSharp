using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._3D
{
	public class Sphere : Shape3D
	{
		public Sphere() : this(0)
		{
		}

		public Sphere(float radius) : base(ShapeTypes3D.Sphere)
		{
			Radius = radius;
			IsOrientable = false;
		}

		public float Radius { get; set; }

		public override bool Contains(vec3 p)
		{
			return Utilities.DistanceSquared(Position, p) <= Radius * Radius;
		}
	}
}
