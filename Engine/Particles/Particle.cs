using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Particles
{
	public class Particle : IPositionable3D
	{
		public vec3 Position { get; set; }
		public vec3 Velocity { get; set; }
	}
}
