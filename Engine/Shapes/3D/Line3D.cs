using GlmSharp;

namespace Engine.Shapes._3D
{
	public class Line3D
	{
		public Line3D() : this(vec3.Zero, vec3.Zero)
		{
		}

		public Line3D(vec3 p1, vec3 p2)
		{
			P1 = p1;
			P2 = p2;
		}

		public vec3 P1 { get; set; }
		public vec3 P2 { get; set; }
	}
}
