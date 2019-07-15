using GlmSharp;

namespace Engine.Interfaces._3D
{
	// 3D renderable objects aren't always scalable in 3D (since 3D sprites still use 2D scaling).
	public interface IRenderable3D : ITransformable3D
	{
		bool IsShadowCaster { get; set; }

		mat4 WorldMatrix { get; }

		void RecomputeWorldMatrix();
	}
}
