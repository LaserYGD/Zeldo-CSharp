using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._2D
{
	public class Circle : Shape2D
	{
		public Circle() : this(vec2.Zero, 0)
		{
		}

		public Circle(float radius) : this(vec2.Zero, radius)
		{
		}

		public Circle(vec2 position, float radius) : base(ShapeTypes2D.Circle)
		{
			Position = position;
			Radius = radius;
		}

		public float Radius { get; set; }

		public Circle Clone(float scale)
		{
			var result = new Circle(Radius * scale);
			result.Position = Position * scale;

			return result;
		}

		public override bool Contains(vec2 p)
		{
			float squared = Utilities.DistanceSquared(p, Position);

			return squared <= Radius * Radius;
		}
	}
}
