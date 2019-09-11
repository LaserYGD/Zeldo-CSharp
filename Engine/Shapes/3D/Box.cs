using System;
using GlmSharp;

namespace Engine.Shapes._3D
{
	public class Box : Shape3D
	{
		public Box() : this(0, 0, 0)
		{
		}

		// This constructor creates a cube.
		public Box(float size) : this(size, size, size)
		{
		}

		public Box(float width, float height, float depth) : base(ShapeTypes3D.Box)
		{
			Width = width;
			Height = height;
			Depth = depth;
		}
		
		public float Width { get; set; }
		public float Height { get; set; }
		public float Depth { get; set; }

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
