using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Shaders
{
	public class ShaderAttribute
	{
		public ShaderAttribute(uint count, uint type, uint offset, ShaderAttributeFlags flags)
		{
			Count = count;
			Type = type;
			Offset = offset;
			IsInteger = (flags & ShaderAttributeFlags.IsInteger) > 0;
			IsNormalized = (flags & ShaderAttributeFlags.IsNormalized) > 0;
		}

		public uint Count { get; }
		public uint Type { get; }
		public uint Offset { get; }

		public bool IsInteger { get; }
		public bool IsNormalized { get; }
	}
}
