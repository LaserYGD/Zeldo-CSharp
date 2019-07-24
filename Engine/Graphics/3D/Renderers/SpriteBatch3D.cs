using Engine.Core._3D;
using Engine.Shaders;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics._3D.Renderers
{
	public class SpriteBatch3D : AbstractRenderer3D<uint, Sprite3D>
	{
		private Shader shader;

		private uint bufferId;

		public unsafe SpriteBatch3D()
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
		}

		public override unsafe void Dispose()
		{
			shader.Dispose();

			fixed (uint* address = &bufferId)
			{
				glDeleteBuffers(1, address);
			}
		}

		public override void Add(Sprite3D sprite)
		{
			Add(sprite.Source.Id, sprite);
		}

		public override void Remove(Sprite3D sprite)
		{
			Remove(sprite.Source.Id, sprite);
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

			foreach (var sprite in Map[key])
			{
			}
		}
	}
}
