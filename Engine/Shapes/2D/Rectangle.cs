using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._2D
{
	public class Rectangle : Shape2D
	{
		public Rectangle() : this(0, 0)
		{
		}

		public Rectangle(float width, float height) : base(ShapeTypes2D.Rectangle)
		{
			Width = width;
			Height = height;
		}

		public float Width { get; set; }
		public float Height { get; set; }

		public float Left
		{
			get => position.x - Width / 2;
			set => position.x = value + Width / 2;
		}

		public float Right
		{
			get => position.x + Width / 2;
			set => position.x = value - Width / 2;
		}

		public float Top
		{
			get => position.y - Height / 2;
			set => position.y = value + Height / 2;
		}

		public float Bottom
		{
			get => position.y + Height / 2;
			set => position.y = value - Height / 2;
		}

		public override bool Contains(vec2 p)
		{
			if (Rotation != 0)
			{
				p = Utilities.Rotate(p, -Rotation);
			}

			return p.x >= Left && p.x <= Right && p.y >= Top && p.y <= Bottom;
		}
	}
}
