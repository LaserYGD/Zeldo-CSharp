using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Zeldo.Physics
{
	public class VerletPoint : IPositionable3D
	{
		public vec3 Position { get; set; }

		public bool Fixed { get; set; }
	}
}
