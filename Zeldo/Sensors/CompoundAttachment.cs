using Engine.Shapes._3D;
using GlmSharp;

namespace Zeldo.Sensors
{
	public class CompoundAttachment
	{
		public CompoundAttachment(Shape3D shape, vec3 position, quat orientation)
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
