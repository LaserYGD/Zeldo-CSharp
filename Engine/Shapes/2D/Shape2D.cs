using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using GlmSharp;

namespace Engine.Shapes._2D
{
	public abstract class Shape2D : IPositionable2D, IRotatable
	{
		protected Shape2D(ShapeTypes2D shapeType)
		{
			ShapeType = shapeType;
		}

		public ShapeTypes2D ShapeType { get; set; }

		public vec2 Position { get; set; }

		public float Rotation { get; set; }
	}
}
