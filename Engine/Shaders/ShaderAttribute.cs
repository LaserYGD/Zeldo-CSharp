using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Shaders
{
	public class ShaderAttribute
	{
		public ShaderAttribute(uint count, uint type, uint offset, bool normalized)
		{
			Count = count;
			Type = type;
			Offset = offset;
			Normalized = normalized;
		}

		public uint Count { get; }
		public uint Type { get; }
		public uint Offset { get; }

		public bool Normalized { get; }
	}
}
