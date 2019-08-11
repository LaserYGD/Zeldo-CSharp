using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeldo.Physics
{
	public static class PhysicsConstants
	{
		static PhysicsConstants()
		{
			WallRange = 0.2f;
		}

		public static float WallRange { get; }
	}
}
