using GlmSharp;

namespace Engine.Shapes._3D
{
	// Note that for lines, orientation is always left as identity.
	public class Line3D : Shape3D
	{
		public Line3D() : this(vec3.Zero, vec3.Zero)
		{
		}

		public Line3D(vec3 p1, vec3 p2) : base(ShapeTypes3D.Line)
		{
			P1 = p1;
			P2 = p2;
		}

		public vec3 P1
		{
			get => Position;
			set => Position = value;
		}

		public vec3 P2 { get; set; }

		public override bool Contains(vec3 p)
		{
			// TODO: Finish this implementation.
			return false;
		}
	}
}
