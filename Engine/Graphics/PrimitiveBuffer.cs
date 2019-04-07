using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Engine.GL;

namespace Engine.Graphics
{
	public class PrimitiveBuffer
	{
		private const ushort RestartIndex = 65535;

		private static readonly uint[] restartModes =
		{
			GL_LINE_LOOP,
			GL_LINE_STRIP,
			GL_TRIANGLE_FAN,
			GL_TRIANGLE_STRIP
		};

		private int bufferSize;
		private int maxIndex;

		private byte[] buffer;
		private ushort[] indexBuffer;
		private bool primitiveRestartEnabled;

		public unsafe PrimitiveBuffer(int bufferCapacity, int indexCapacity)
		{
			buffer = new byte[bufferCapacity];
			indexBuffer = new ushort[indexCapacity];

			uint[] buffers = new uint[2];

			fixed (uint* address = &buffers[0])
			{
				glGenBuffers(2, address);
			}

			BufferId = buffers[0];
			IndexBufferId = buffers[1];

			glBindBuffer(GL_ARRAY_BUFFER, BufferId);
			glBufferData(GL_ARRAY_BUFFER, (uint)buffer.Length, null, GL_DYNAMIC_DRAW);

			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, IndexBufferId);
			glBufferData(GL_ELEMENT_ARRAY_BUFFER, (uint)indexBuffer.Length, null, GL_DYNAMIC_DRAW);
		}

		public int Size => bufferSize;
		public int IndexCount { get; private set; }

		public uint Mode
		{
			set => primitiveRestartEnabled = restartModes.Contains(value);
		}

		public uint BufferId { get; }
		public uint IndexBufferId { get; }

		public void Buffer<T>(T[] data, uint stride, int start = 0, int length = -1) where T : struct
		{
			int size = Marshal.SizeOf(typeof(T));
			int sizeInBytes = size * (length != -1 ? length : data.Length);

			// See https://stackoverflow.com/a/4636735/7281613.
			System.Buffer.BlockCopy(data, start * size, buffer, bufferSize, sizeInBytes);

			// Vertex count is implied through the data given (it's assumed that the data array will always be the
			// correct length based on the current shader from the calling code).
			int vertexCount = sizeInBytes / (int)stride;

			for (int i = 0; i < vertexCount; i++)
			{
				indexBuffer[IndexCount + i] = (ushort)(maxIndex + i);
			}

			bufferSize += sizeInBytes;
			IndexCount += vertexCount;
			maxIndex += (ushort)vertexCount;

			if (primitiveRestartEnabled)
			{
				indexBuffer[IndexCount] = RestartIndex;
				IndexCount++;
			}
		}

		public unsafe uint Flush()
		{
			if (primitiveRestartEnabled)
			{
				glEnable(GL_PRIMITIVE_RESTART);
			}
			else
			{
				glDisable(GL_PRIMITIVE_RESTART);
			}

			fixed (byte* address = &buffer[0])
			{
				glBufferSubData(GL_ARRAY_BUFFER, 0, (uint)bufferSize, address);
			}

			fixed (ushort* address = &indexBuffer[0])
			{
				glBufferSubData(GL_ELEMENT_ARRAY_BUFFER, 0, sizeof(ushort) * (uint)IndexCount, address);
			}

			uint count = (uint)IndexCount;

			bufferSize = 0;
			IndexCount = 0;
			maxIndex = 0;

			return count;
		}
	}
}
