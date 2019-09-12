using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Shapes._3D
{
	public abstract class Shape3D : ITransformable3D
	{
		protected Shape3D(ShapeTypes3D shapeType)
		{
			ShapeType = shapeType;
			Orientation = quat.Identity;
		}

		public ShapeTypes3D ShapeType { get; }

		public vec3 Position { get; set; }
		public quat Orientation { get; set; }

		// This value allows overlap calculations to be optimized in many cases.
		public bool IsAxisAligned { get; set; }

		public void SetTransform(vec3 position, quat orientation)
		{
			Position = position;
			Orientation = orientation;
		}

		public abstract bool Contains(vec3 p);

		public bool Overlaps(Shape3D other)
		{
			return ShapeHelper3D.Overlaps(this, other);
		}
	}
}
