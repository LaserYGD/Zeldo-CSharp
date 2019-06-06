using GlmSharp;

namespace Engine.Shapes._2D
{
	public class Line2D : Shape2D
	{
		public Line2D() : this(vec2.Zero, vec2.Zero)
		{
		}

		public Line2D(vec2 p1, vec2 p2) : base(ShapeTypes2D.Line)
		{
			P1 = p1;
			P2 = p2;
		}

		public vec2 P1
		{
			get => Position;
			set => Position = value;
		}

		public vec2 P2 { get; set; }

		public Line2D Clone(float scale)
		{
			return new Line2D(P1 * scale, P2 * scale);
		}

		public override bool Contains(vec2 p)
		{
			return false;
		}
	}
}
