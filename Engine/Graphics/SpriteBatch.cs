using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Shaders;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics
{
	public class SpriteBatch : IReceiver
	{
		private Shader spriteShader;
		private Shader primitiveShader;
		private Shader activeShader;
		private PrimitiveBuffer buffer;
		private mat4 mvp;

		private uint mode;
		private uint activeTexture;

		public SpriteBatch()
		{
			buffer = new PrimitiveBuffer(4096, 512);

			// These two shaders (owned by the sprite batch) can be completed here (in terms of binding a buffer).
			// External shaders are bound when first applied.
			spriteShader = new Shader();
			spriteShader.Attach(ShaderTypes.Vertex, "Sprite.vert");
			spriteShader.Attach(ShaderTypes.Fragment, "Sprite.frag");
			spriteShader.CreateProgram();
			spriteShader.AddAttribute<float>(2, GL_FLOAT);
			spriteShader.AddAttribute<float>(2, GL_FLOAT);
			spriteShader.AddAttribute<byte>(4, GL_UNSIGNED_BYTE, true);
			spriteShader.Bind(buffer);

			primitiveShader = new Shader();
			primitiveShader.Attach(ShaderTypes.Vertex, "Primitives2D.vert");
			primitiveShader.Attach(ShaderTypes.Fragment, "Primitives.frag");
			primitiveShader.CreateProgram();
			primitiveShader.AddAttribute<float>(2, GL_FLOAT);
			primitiveShader.AddAttribute<byte>(4, GL_UNSIGNED_BYTE, true);
			primitiveShader.Bind(buffer);

			MessageSystem.Subscribe(this, CoreMessageTypes.Resize, (messageType, data, dt) => { OnResize(); });
		}

		public uint Mode
		{
			get => mode;
			set
			{
				if (mode != value)
				{
					Flush();
				}

				mode = value;
				buffer.Mode = value;
			}
		}

		public List<MessageHandle> MessageHandles { get; set; }

		private void OnResize()
		{
			var halfDimensions = Resolution.Dimensions / 2;

			mvp = mat4.Scale(1f / halfDimensions.x, 1f / halfDimensions.y, 1);
			mvp *= mat4.Translate(-halfDimensions.x, -halfDimensions.y, 0);
		}

		public void Buffer(float[] data, int start = 0, int length = -1)
		{
			if (activeShader == null)
			{
				activeShader = spriteShader;
			}
			
			buffer.Buffer(data, activeShader.Stride, start, length);
		}

		public void Apply(Shader shader, uint mode)
		{
			if (activeShader == shader && this.mode == mode)
			{
				return;
			}

			Flush();
			activeShader = shader;

			this.mode = mode;

			if (!activeShader.IsBindingComplete)
			{
				activeShader.Bind(buffer);
			}
		}

		public void BindTexture(uint id)
		{
			if (activeTexture == id)
			{
				return;
			}

			Flush();
			activeTexture = id;
		}

		public void DrawLine(ivec2 p1, ivec2 p2, Color color)
		{
			Apply(primitiveShader, GL_LINES);

			float f = color.ToFloat();
			float[] data =
			{
				p1.x,
				p1.y,
				f,
				p2.x,
				p2.y,
				f
			};

			Buffer(data);
		}

		public unsafe void Flush()
		{
			if (buffer.Size == 0)
			{
				return;
			}

			// This assumes that all 2D shaders will contain a uniform matrix called "mvp".
			activeShader.Apply();
			activeShader.SetUniform("mvp", mvp);

			if (activeTexture != 0)
			{
				glActiveTexture(GL_TEXTURE0);
				glBindTexture(GL_TEXTURE_2D, activeTexture);
			}

			glDrawElements(mode, buffer.Flush(), GL_UNSIGNED_SHORT, null);

			activeShader = null;
			activeTexture = 0;
		}
	}
}
