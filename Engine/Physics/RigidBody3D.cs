using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Physics
{
	public class RigidBody3D : ITransformable3D
	{
		public vec3 Position { get; set; }
		public quat Orientation { get; set; }

		public void SetTransform(vec3 position, quat orientation)
		{
		}
	}
}
