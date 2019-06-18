using GlmSharp;

namespace Engine.Shapes._2D
{
	public class Point : Shape2D
	{
		public Point() : this(vec2.Zero)
		{
		}

		public Point(vec2 position) : base(ShapeTypes2D.Point)
		{
			Position = position;
		}

		public override bool Contains(vec2 p)
		{
			return Position == p;
		}
	}
}
