using System;
using System.Collections.Generic;
using Engine.Interfaces._3D;
using Engine.Lighting;
using Engine.Shaders;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics._3D.Rendering
{
	public abstract class AbstractRenderer3D<T> : IDisposable where T : IRenderable3D
	{
		private uint bufferId;
		private uint indexId;
		private uint shadowVao;

		protected AbstractRenderer3D(GlobalLight light)
		{
			Light = light;
		}

		protected Shader Shader { get; set; }
		protected GlobalLight Light { get; }

		protected unsafe void Bind(Shader shader, uint bufferId, uint indexId = 0)
		{
			this.bufferId = bufferId;
			this.indexId = indexId;

			shader.Bind(bufferId, indexId);
			Shader = shader;
			
			uint stride = shader.Stride;

			fixed (uint* address = &shadowVao)
			{
				glGenVertexArrays(1, address);
			}

			glBindVertexArray(shadowVao);
			glVertexAttribPointer(0, 3, GL_FLOAT, false, stride, (void*)0);
			//glVertexAttribPointer(0, 3, GL_FLOAT, false, stride, (void*)0);
			//glVertexAttribPointer(1, 2, GL_FLOAT, false, stride, (void*)(stride - sizeof(float) * 5));
			glEnableVertexAttribArray(0);

			/*
			for (int i = 0; i <= 1; i++)
			{
				glEnableVertexAttribArray((uint)i);
			}
			*/
		}

		public unsafe void Dispose()
		{
			Shader.Dispose();

			// The index buffer ID isn't bound for 3D sprites (which don't use indices for rendering).
			if (indexId == 0)
			{
				fixed (uint* address = &bufferId)
				{
					glDeleteBuffers(1, address);
				}

				return;
			}

			uint[] buffers =
			{
				bufferId,
				indexId
			};

			fixed (uint* address = &buffers[0])
			{
				glDeleteBuffers(2, address);
			}
		}

		public abstract List<T> RetrieveNext();

		public abstract void Add(T item);
		public abstract void Remove(T item);
		public abstract void Prepare();

		public virtual void PrepareShadow()
		{
			glBindVertexArray(shadowVao);
			glBindBuffer(GL_ARRAY_BUFFER, bufferId);
			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);
		}

		public abstract void Draw(T item, mat4? vp);
	}
}
