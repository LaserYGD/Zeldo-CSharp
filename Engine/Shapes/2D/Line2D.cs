using GlmSharp;

namespace Engine.Shapes._2D
{
	public class Line2D
	{
		public Line2D() : this(vec2.Zero, vec2.Zero)
		{
		}

		public Line2D(vec2 p1, vec2 p2)
		{
			P1 = p1;
			P2 = p2;
		}

		public vec2 P1 { get; set; }
		public vec2 P2 { get; set; }
	}
}
