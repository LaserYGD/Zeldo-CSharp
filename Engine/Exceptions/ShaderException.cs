using System;
using Engine.Shaders;

namespace Engine.Exceptions
{
	public class ShaderException : Exception
	{
		public ShaderException(ShaderStages stage, string message) : base($"[{stage}] {message}")
		{
		}
	}
}
