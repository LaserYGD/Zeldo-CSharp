using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Structures
{
	public class SpringPoint3D : IPositionable3D, IDynamic
	{
		private float k;
		private float damping;

		private vec3 velocity;

		public SpringPoint3D(float k, float damping) : this(vec3.Zero, k, damping)
		{
		}

		public SpringPoint3D(vec3 position, float k, float damping)
		{
			this.k = k;
			this.damping = damping;

			// Each spring starts with its position and target being equal (which results in no velocity change).
			Position = position;
			Target = position;
		}

		public vec3 Position { get; set; }
		public vec3 Target { get; set; }

		public void Update(float dt)
		{
			// See https://en.wikipedia.org/wiki/Hooke%27s_law (F = kx).
			velocity += (Target - Position) * k;
			Position += velocity * dt;
			velocity *= damping;
		}
	}
}
