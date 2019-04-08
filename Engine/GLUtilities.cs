using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Engine.GL;

namespace Engine
{
	public static class GLUtilities
	{
		public static unsafe void AllocateBuffers(int bufferCapacity, int indexCapacity, out uint bufferId,
			out uint indexBufferId)
		{
			uint[] buffers = new uint[2];

			fixed (uint* address = &buffers[0])
			{
				glGenBuffers(2, address);
			}

			bufferId = buffers[0];
			indexBufferId = buffers[1];

			// Note that buffer capacity should be given in bytes, while index capacity should be given in indexes
			// (i.e. unsigned shorts). This is meant to match how primitive buffers are created.
			glBindBuffer(GL_ARRAY_BUFFER, bufferId);
			glBufferData(GL_ARRAY_BUFFER, (uint)bufferCapacity, null, GL_DYNAMIC_DRAW);

			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexBufferId);
			glBufferData(GL_ELEMENT_ARRAY_BUFFER, (uint)indexCapacity * sizeof(ushort), null, GL_DYNAMIC_DRAW);
		}
	}
}
