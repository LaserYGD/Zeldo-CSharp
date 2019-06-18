using Engine.Shapes._2D;
using GlmSharp;

namespace Zeldo.Sensors
{
	public class CompoundAttachment
	{
		public CompoundAttachment(Shape2D shape, float height, vec2 position, float elevation, float rotation)
		{
			Shape = shape;
			Position = position;
			Height = height;
			Elevation = elevation;
			Rotation = rotation;
		}

		public Shape2D Shape { get; }
		public vec2 Position { get; }

		public float Height { get; }
		public float Elevation { get; }
		public float Rotation { get; }
	}
}
