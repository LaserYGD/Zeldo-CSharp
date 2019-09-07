using Engine.Shaders;
using GlmSharp;

namespace Engine.Interfaces._3D
{
	// 3D renderable objects aren't always scalable in 3D (since 3D sprites still use 2D scaling).
	public interface IRenderable3D : ITransformable3D
	{
		bool IsShadowCaster { get; set; }

		mat4 WorldMatrix { get; }

		// Setting custom shaders is optional. Shadow shaders are only required if vertices for the renderable object
		// are transformed in distinct ways on the base shader.
		Shader Shader { get; set; }
		Shader ShadowShader { get; set; }

		void RecomputeWorldMatrix();
	}
}
