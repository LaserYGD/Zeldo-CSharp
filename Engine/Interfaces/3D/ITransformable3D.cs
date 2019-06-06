using GlmSharp;

namespace Engine.Interfaces._3D
{
	public interface ITransformable3D : IPositionable3D, IOrientable
	{
		void SetTransform(vec3 position, quat orientation);
	}
}
