using System.Diagnostics;
using Engine.Interfaces._2D;
using GlmSharp;

namespace Engine.Physics.Verlet
{
	[DebuggerDisplay("P={Position}, Old={OldPosition}, Rotation={Rotation}")]
	public class VerletPoint2D : IPositionable2D, IRotatable
	{
		public VerletPoint2D() : this(vec2.Zero)
		{
		}

		public VerletPoint2D(vec2 p)
		{
			Position = p;
			OldPosition = p;
		}

		public vec2 Position { get; set; }
		public vec2 OldPosition { get; internal set; }

		public float Rotation { get; set; }
	}
}
