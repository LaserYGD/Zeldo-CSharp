using Engine.Shapes._3D;
using GlmSharp;

namespace Engine.Sensors
{
	public class ShapeAttachment
	{
		public ShapeAttachment(Shape3D shape, vec3 position, quat orientation)
		{
			Shape = shape;
			Position = position;
			Orientation = orientation;
		}

		public Shape3D Shape { get; }
		public vec3 Position { get; }
		public quat Orientation { get; }
	}
}
