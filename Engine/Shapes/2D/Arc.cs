using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Utility;
using GlmSharp;

namespace Engine.Shapes._2D
{
	public class Arc : Shape2D
	{
		public Arc() : this(0, 0, 0)
		{
		}

		public Arc(float radius, float spread) : this(0, radius, spread)
		{
		}

		public Arc(float angle, float radius, float spread) : base(ShapeTypes2D.Arc)
		{
			Rotation = angle;
			Radius = radius;
			Spread = spread;
		}

		public float Radius { get; set; }
		public float Spread { get; set; }
		public float Angle
		{
			get => Rotation;
			set => Rotation = value;
		}

		public override bool Contains(vec2 p)
		{
			if (Utilities.DistanceSquared(position, p) > Radius * Radius)
			{
				return false;
			}

			float angle = Utilities.Angle(position, p);
			float delta = Utilities.CorrectAngle(Math.Abs(angle - Angle));

			return delta <= Spread / 2;
		}
	}
}
