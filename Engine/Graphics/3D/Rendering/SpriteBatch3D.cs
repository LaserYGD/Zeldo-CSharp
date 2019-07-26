using Engine.Core._3D;
using Engine.Lighting;
using Engine.Shaders;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics._3D.Rendering
{
	public class SpriteBatch3D : MapRenderer3D<uint, Sprite3D>
	{
		public unsafe SpriteBatch3D(GlobalLight light) : base(light)
		{
			uint bufferId;
			
			glGenBuffers(1, &bufferId);

			var shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "Sprite3D.vert");
			shader.Attach(ShaderTypes.Fragment, "Sprite3D.frag");
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.Initialize();
			shader.Use();
			shader.SetUniform("shadowSampler", 0);
			shader.SetUniform("textureSampler", 1);

			Bind(shader, bufferId);

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

			base.PrepareShadow();
		}

		public override void Prepare()
		{
			glDisable(GL_CULL_FACE);
		}

		protected override void Apply(uint key)
		{
			// For 3D sprites, the key is the source ID.
			glBindTexture(GL_TEXTURE0, key);
		}

		public override void Draw(Sprite3D item, mat4? vp)
		{
		}
	}
}
