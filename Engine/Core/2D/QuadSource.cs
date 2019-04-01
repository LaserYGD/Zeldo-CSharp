using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Core._2D
{
	public abstract class QuadSource
	{
		protected QuadSource(uint id, uint width, uint height)
		{
			Id = id;
			Width = width;
			Height = height;
		}

		public uint Id { get; }
		public uint Width { get; }
		public uint Height { get; }
	}
}
