using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Lighting
{
	public class Spotlight : IOrientable
	{
		public quat Orientation { get; set; }

		public float Spread { get; set; }
	}
}
