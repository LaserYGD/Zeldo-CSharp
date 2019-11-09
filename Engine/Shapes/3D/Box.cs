using System;
using GlmSharp;

namespace Engine.Shapes._3D
{
	public class Box : Shape3D
	{
		public Box() : this(0, 0, 0)
		{
		}

		// This constructor creates a cube (i.e. all dimensions the same size).
		public Box(float size, BoxFlags flags = BoxFlags.IsOrientable) : this(size, size, size, flags)
		{
		}

		public Box(float width, float height, float depth, BoxFlags flags = BoxFlags.IsOrientable) :
			base(ShapeTypes3D.Box)
		{
			Width = width;
			Height = height;
			Depth = depth;
			IsOrientable = (flags & BoxFlags.IsOrientable) > 0;
			IsFixedVertical = (flags & BoxFlags.IsFixedVertical) > 0;
		}

		public float Width { get; set; }
		public float Height { get; set; }
		public float Depth { get; set; }

		public vec3 Bounds
		{
			get => new vec3(Width, Height, Depth);
			set
			{
				Width = value.x;
				Height = value.y;
				Depth = value.z;
			}
		}

		// Just like the orientable flag, this allows optimizations in overlap code.
		public bool IsFixedVertical { get; set; }

		public override bool Contains(vec3 p)
		{
			if (Orientation != quat.Identity)
			{
				p = Orientation.Inverse * p;
			}

			float dX = Math.Abs(Position.x - p.x);
			float dY = Math.Abs(Position.y - p.y);
			float dZ = Math.Abs(Position.z - p.z);

			return dX <= Width / 2 && dY <= Height / 2 && dZ <= Depth / 2;
		}
	}
}
