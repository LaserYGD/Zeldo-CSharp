using Engine.Core;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Lighting
{
	public abstract class LightSource : IPositionable3D, IColorable
	{
		public vec3 Position { get; set; }
		public Color Color { get; set; } = Color.White;

		public float Range { get; set; }
	}
}
