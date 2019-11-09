using System.Diagnostics;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Shapes._3D
{
	public abstract class Shape3D : ITransformable3D
	{
		private vec3 position;
		private quat orientation;

		protected Shape3D(ShapeTypes3D shapeType)
		{
			ShapeType = shapeType;
			Orientation = quat.Identity;
		}

		public ShapeTypes3D ShapeType { get; }

		public vec3 Position
		{
			get => position;
			set => position = value;
		}

		public quat Orientation
		{
			get => orientation;
			set
			{
				// Ideally, this property would never be called for non-orientable shapes. In practice, though, that's
				// difficult (and likely inefficient) to enforce. Much simpler to just return.
				if (!IsOrientable)
				{
					return;
				}

				orientation = value;
			}
		}

		// This allows overlap calculations to be optimized in many cases. It's also permanently disabled for some
		// shapes (like lines and spheres).
		public bool IsOrientable { get; set; }

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
