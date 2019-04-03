using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Shapes._3D
{
	public class Cube : Shape3D
	{
		public Cube() : this(0)
		{
		}

		public Cube(float halfSize) : base(ShapeTypes3D.Cube)
		{
			HalfSize = halfSize;
		}

		// A cube has the same size on each edge. Half size, then, is roughly equivalent to the radius of a sphere.
		public float HalfSize { get; set; }
	}
}
