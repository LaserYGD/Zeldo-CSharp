using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Physics.Verlet
{
	public class VerletPoint : IPositionable3D
	{
		public vec3 Position { get; set; }
		public vec3 OldPosition { get; set; }
	}
}
