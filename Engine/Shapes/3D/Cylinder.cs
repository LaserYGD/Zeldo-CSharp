using System;
using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._3D
{
	public class Cylinder : Shape3D
	{
		public Cylinder() : this(0, 0)
		{
		}

		public Cylinder(float height, float radius) : base(ShapeTypes3D.Cylinder)
		{
			Height = height;
			Radius = radius;
		}

		public float Height { get; set; }
		public float Radius { get; set; }

		public override bool Contains(vec3 p)
		{
			if (!IsAxisAligned)
			{
				p = Orientation.Inverse * p;
			}

			float dY = Math.Abs(Position.y - p.y);

			if (dY > Height / 2)
			{
				return false;
			}

			float squared = Utilities.DistanceSquared(Position.swizzle.xz, p.swizzle.xz);

			return squared <= Radius * Radius;
		}
	}
}
