using System;

namespace Engine.Shaders
{
	[Flags]
	public enum ShaderAttributeFlags
	{
		None = 0,
		IsInteger = 1<<0,
		IsNormalized= 1<<1
	}
}
