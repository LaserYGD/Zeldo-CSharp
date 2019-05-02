using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;
using Jitter.LinearMath;

namespace Engine.Physics
{
	public static class PhysicsExtensions
	{
		public static vec3 ToVec3(this JVector v)
		{
			return new vec3(v.X, v.Y, v.Z);
		}

		public static quat ToQuat(this JMatrix m)
		{
			var q = JQuaternion.CreateFromMatrix(m);

			return new quat(q.X, q.Y, q.Z, q.W);
		}
	}
}
