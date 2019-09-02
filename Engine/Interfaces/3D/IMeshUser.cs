using Engine.Graphics._3D;

namespace Engine.Interfaces._3D
{
	// This interface aids rendering (since models and skeletons both use meshes).
	public interface IMeshUser : IRenderable3D
	{
		Mesh Mesh { get; }
	}
}
