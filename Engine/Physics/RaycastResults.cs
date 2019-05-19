using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;
using Jitter.Dynamics;

namespace Engine.Physics
{
	public class RaycastResults
	{
		public RaycastResults(RigidBody body, vec3 position, vec3 normal)
		{
			Body = body;
			Position = position;
			Normal = normal;
		}

		public RigidBody Body { get; }
		public vec3 Position { get; }
		public vec3 Normal { get; }
	}
}
