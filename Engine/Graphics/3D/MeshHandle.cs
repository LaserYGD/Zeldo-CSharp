using System.Collections.Generic;
using Engine.Core._3D;

namespace Engine.Graphics._3D
{
	public class MeshHandle
	{
		public MeshHandle(int count, int offset, int baseVertex)
		{
			Count = count;
			Offset = offset;
			BaseVertex = baseVertex;
			Models = new List<Model>();
		}

		public List<Model> Models { get; }
		
		public int Count { get; }
		public int Offset { get; }
		public int BaseVertex { get; }
	}
}
