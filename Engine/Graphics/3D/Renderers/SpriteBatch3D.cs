using System;
using System.Collections.Generic;
using Engine.Core._3D;
using Engine.Shaders;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics._3D.Renderers
{
	public class SpriteBatch3D : AbstractRenderer3D<uint, Sprite3D>
	{
		// This maps source IDs (textures or render targets) to groups of sprites (for instancing).
		private Dictionary<uint, List<Sprite3D>> map;
		private List<uint> idList;
		private Shader shader;

		private uint bufferId;
		private int nextIndex;

		public unsafe SpriteBatch3D(MasterRenderer3D parent) : base(parent)
		{
			fixed (uint* address = &bufferId)
			{
				glGenBuffers(1, address);
			}

			shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "Sprite3D.vert");
			shader.Attach(ShaderTypes.Fragment, "Sprite3D.frag");
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.CreateProgram();
			shader.Bind(bufferId, indexId);
			shader.Use();
			shader.SetUniform("shadowSampler", 0);
			shader.SetUniform("textureSampler", 1);

			vec2[] points =
			{
				new vec2(-1, -1),
				new vec2(1, -1),
				new vec2(1, 1),
				new vec2(-1, 1)
			};

			glBindBuffer(GL_ARRAY_BUFFER, bufferId);

			fixed (float* address = &points[0].x)
			{
				glBufferData(GL_ARRAY_BUFFER, sizeof(float) * 8, address, GL_STATIC_DRAW);
			}

			map = new Dictionary<uint, List<Sprite3D>>();
		}

		public override unsafe void Dispose()
		{
			shader.Dispose();

			fixed (uint* address = &bufferId)
			{
				glDeleteBuffers(1, address);
			}
		}

		public void Add(Sprite3D sprite)
		{
			Add(sprite.Source.Id, sprite);
		}

		public void Remove(Sprite3D sprite)
		{
			Map[sprite.Source.Id].Remove(sprite);
		}

		public override void PrepareShadow()
		{
			glDisable(GL_CULL_FACE);
		}

		public override void Prepare()
		{
			glDisable(GL_CULL_FACE);
		}

		public override void Draw(uint key)
		{
			// For 3D sprites, the key is the source ID.
			glBindTexture(GL_TEXTURE0, key);

			foreach (var sprite in map[key])
			{
			}
		}
	}
}
