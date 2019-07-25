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
		protected AbstractRenderer3D(GlobalLight light)
		{
			Light = light;
		}

		protected Shader Shader { get; set; }
		protected GlobalLight Light { get; }

		public uint ShadowVao { get; private set; }

		protected unsafe void GenerateShadowVao(uint bufferId, uint indexId = 0)
		{
			uint vao;
			uint stride = shader.Stride;

			glGenVertexArrays(1, &vao);
			glBindVertexArray(vao);
			glBindBuffer(GL_ARRAY_BUFFER, bufferId);
			glVertexAttribPointer(0, 3, GL_FLOAT, false, stride, (void*)0);
			glVertexAttribPointer(1, 2, GL_FLOAT, false, stride, (void*)(stride - sizeof(float) * 5));

			if (indexId != 0)
			{
				glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);
			}

			for (int i = 0; i <= 1; i++)
			{
				glEnableVertexAttribArray((uint)i);
			}

			ShadowVao = vao;
		}

		public void Dispose()
		{
			Shader.Dispose();

			// TODO: Delete buffers as well.
		}

		public abstract List<T> RetrieveNext();

		public abstract void Add(T item);
		public abstract void Remove(T item);
		public abstract void Prepare();
		public abstract void PrepareShadow();
		public abstract void Draw(T item, mat4? vp);
	}
}
