using System;
using System.Collections.Generic;
using Engine.Interfaces._3D;
using Engine.Lighting;
using Engine.Shaders;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics._3D.Rendering
{
	// TODO: Move map renderer functionality down here, and convert the existing MapRenderer to a mesh renderer.
	public abstract class AbstractRenderer3D<K, V> : IDisposable where V : IRenderable3D
	{
		protected uint bufferId;
		protected uint indexId;

		protected Shader shader;

		private uint shadowVao;
		private int nextIndex;

		private Dictionary<K, List<V>> map;

		// A separate list is needed to access keys by index.
		private List<K> keys;

		protected AbstractRenderer3D(GlobalLight light)
		{
			Light = light;
			map = new Dictionary<K, List<V>>();
			keys = new List<K>();
		}

		protected GlobalLight Light { get; }

		protected unsafe void Bind(uint bufferId, uint indexId)
		{
			this.bufferId = bufferId;
			this.indexId = indexId;

			shader.Initialize();
			shader.Bind(bufferId, indexId);
			shader.Use();
			shader.SetUniform("textureSampler", 0);
			shader.SetUniform("shadowSampler", 1);
			
			uint stride = shader.Stride;

			fixed (uint* address = &shadowVao)
			{
				glGenVertexArrays(1, address);
			}

			glBindVertexArray(shadowVao);
			glVertexAttribPointer(0, 3, GL_FLOAT, false, stride, (void*)0);
			glVertexAttribPointer(1, 2, GL_FLOAT, false, stride, (void*)(sizeof(float) * 3));

			for (int i = 0; i <= 1; i++)
			{
				glEnableVertexAttribArray((uint)i);
			}
		}

		public unsafe void Dispose()
		{
			shader.Dispose();

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

		public abstract void Add(V item);
		public abstract void Remove(V item);

		protected void Add(K key, V item)
		{
			if (!map.TryGetValue(key, out var list))
			{
				list = new List<V>();
				map.Add(key, list);
				keys.Add(key);
			}

			list.Add(item);
		}

		protected void Remove(K key, V item)
		{
			map[key].Remove(item);
		}

		public List<V> RetrieveNext()
		{
			if (nextIndex < keys.Count)
			{
				K key = keys[nextIndex++];

				// This call allows binding any relevant open GL state before drawing begins.
				Apply(key);

				return map[key];
			}

			// This resets the renderer for the next phase.
			nextIndex = 0;

			return null;
		}

		protected abstract void Apply(K key);

		public virtual void PrepareShadow()
		{
			glBindVertexArray(shadowVao);
			glBindBuffer(GL_ARRAY_BUFFER, bufferId);
			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);
		}

		public virtual void Prepare()
		{
			shader.Apply();
			shader.SetUniform("lightDirection", Light.Direction);
			shader.SetUniform("lightColor", Light.Color.ToVec3());
			shader.SetUniform("ambientIntensity", Light.AmbientIntensity);
		}

		protected void PrepareShader(V item, mat4? vp)
		{
			if (vp.HasValue)
			{
				mat4 world = item.WorldMatrix;
				quat orientation = item.Orientation;

				shader.SetUniform("orientation", orientation.ToMat4);
				shader.SetUniform("mvp", vp.Value * world);
				shader.SetUniform("lightBiasMatrix", Light.BiasMatrix * world);
			}
		}

		public abstract void Draw(V item, mat4? vp);
	}
}
