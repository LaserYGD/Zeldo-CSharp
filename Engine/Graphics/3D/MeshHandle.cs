using Engine.Core._3D;

namespace Engine.Graphics._3D
{
	public class MeshHandle
	{
		public MeshHandle(Model model, int count, int offset, int baseVertex)
		{
			Model = model;
			Count = count;
			Offset = offset;
			BaseVertex = baseVertex;
		}

		public Model Model { get; }
		
		public int Count { get; }
		public int Offset { get; }
		public int BaseVertex { get; }
	}
}
