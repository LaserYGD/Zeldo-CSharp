using System.Linq;
using Engine.Core;
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
			GLUtilities.GenerateBuffers(out uint bufferId, out uint indexId);

			var shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "Sprite3D.vert");
			shader.Attach(ShaderTypes.Fragment, "Sprite3D.frag");
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.Initialize();
			shader.Use();
			shader.SetUniform("textureSampler", 0);
			shader.SetUniform("shadowSampler", 1);

			Bind(shader, bufferId, indexId);

			// Buffer vertex data.
			vec2[] points =
			{
				-vec2.Ones,
				new vec2(-1, 1),
				new vec2(1, -1),
				vec2.Ones
			};

			// Dividing each point by two gives the entire quad a unit length of one.
			for (int i = 0; i < points.Length; i++)
			{
				points[i] /= 2;
			}

			// Modifying the source order causes sprites to appear the right way up (and not flipped horizontally).
			vec2[] sources =
			{
				vec2.UnitY,
				vec2.Zero,
				vec2.Ones,
				vec2.UnitX
			};

			float[] data = new float[20];

			for (int i = 0; i < points.Length; i++)
			{
				var p = points[i];
				var s = sources[i];

				int start = i * 5;

				data[start] = p.x;
				data[start + 1] = p.y;
				data[start + 2] = 0;
				data[start + 3] = s.x;
				data[start + 4] = s.y;
			}

			glBindBuffer(GL_ARRAY_BUFFER, bufferId);

			fixed (float* address = &data[0])
			{
				glBufferData(GL_ARRAY_BUFFER, sizeof(float) * (uint)data.Length, address, GL_STATIC_DRAW);
			}

			// Buffer index data.
			ushort[] indices = new ushort[4];

			for (int i = 0; i < indices.Length; i++)
			{
				indices[i] = (ushort)i;
			}

			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);

			fixed (ushort* address = &indices[0])
			{
				glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(ushort) * 4, address, GL_STATIC_DRAW);
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

			base.Prepare();
		}

		protected override void Apply(uint key)
		{
			// For 3D sprites, the key is the source ID.
			glActiveTexture(GL_TEXTURE0);
			glBindTexture(GL_TEXTURE_2D, key);
		}

		public override unsafe void Draw(Sprite3D item, mat4? vp)
		{
			PrepareShader(item, vp);
			Shader.SetUniform("tint", item.Color.ToVec4());
			
			glDrawElements(GL_TRIANGLE_STRIP, 4, GL_UNSIGNED_SHORT, (void*)0);
		}
	}
}
