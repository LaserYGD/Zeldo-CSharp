using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Shapes._2D
{
	public class Circle : Shape2D
	{
		public Circle() : base(ShapeTypes2D.Circle)
		{
		}

		public float Radius { get; set; }
	}
}
