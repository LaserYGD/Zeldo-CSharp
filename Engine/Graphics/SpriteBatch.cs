using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Shaders;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics
{
	public class SpriteBatch : IReceiver
	{
		private const ushort RestartIndex = 65535;

		private Shader spriteShader;
		private Shader activeShader;
		private mat4 mvp;

		private uint bufferId;
		private uint bufferSize;
		private uint indexBufferId;
		private uint indexCount;
		private ushort maxIndex;
		private uint mode;
		private uint activeTexture;

		private byte[] buffer;
		private ushort[] indexBuffer;
		private bool primitiveRestartEnabled;

		public unsafe SpriteBatch()
		{
			spriteShader = new Shader();
			spriteShader.Attach(ShaderTypes.Vertex, "Sprite.vert");
			spriteShader.Attach(ShaderTypes.Fragment, "Sprite.frag");
			spriteShader.CreateProgram();
			spriteShader.AddAttribute<float>(2, GL_FLOAT);
			spriteShader.AddAttribute<float>(2, GL_FLOAT);
			spriteShader.AddAttribute<byte>(4, GL_UNSIGNED_BYTE, true);

			buffer = new byte[2048];
			indexBuffer = new ushort[2014];

			uint[] buffers = new uint[2];

			fixed (uint* address = &buffers[0])
			{
				glGenBuffers(2, address);
			}

			bufferId = buffers[0];
			indexBufferId = buffers[1];

			glBindBuffer(GL_ARRAY_BUFFER, bufferId);
			glBufferData(GL_ARRAY_BUFFER, 2048, null, GL_DYNAMIC_DRAW);

			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexBufferId);
			glBufferData(GL_ELEMENT_ARRAY_BUFFER, 1024, null, GL_DYNAMIC_DRAW);

			MessageHandles = new List<MessageHandle>();

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

				uint[] restartModes =
				{
					GL_LINE_LOOP,
					GL_LINE_STRIP,
					GL_TRIANGLE_FAN,
					GL_TRIANGLE_STRIP
				};

				primitiveRestartEnabled = restartModes.Contains(mode);
			}
		}

		public List<MessageHandle> MessageHandles { get; set; }

		private void OnResize()
		{
			var halfDimensions = Resolution.Dimensions / 2;

			mvp = mat4.Scale(1f / halfDimensions.x, 1f / halfDimensions.y, 1);
			mvp *= mat4.Translate(-halfDimensions.x, -halfDimensions.y, 0);
		}

		public void Buffer(float[] data)
		{
			int sizeInBytes = sizeof(float) * data.Length;

			// See https://stackoverflow.com/a/4636735/7281613.
			System.Buffer.BlockCopy(data, 0, buffer, (int)bufferSize, sizeInBytes);

			// Vertex count is implied through the data given (it's assumed that the data array will always be the
			// correct length, based on the current shader).
			uint vertexCount = (uint)sizeInBytes / activeShader.Stride;

			for (int i = 0; i < vertexCount; i++)
			{
				indexBuffer[indexCount + i] = (ushort)(maxIndex + i);
			}

			bufferSize += (uint)data.Length;
			indexCount += vertexCount;
			maxIndex += (ushort)vertexCount;

			if (primitiveRestartEnabled)
			{
				indexBuffer[indexCount] = RestartIndex;
				indexCount++;
			}
		}

		public void Apply(Shader shader, uint mode)
		{
			if (activeShader == shader && this.mode == mode)
			{
				return;
			}

			Flush();
			activeShader = shader;
			Mode = mode;

			if (!activeShader.IsBindingComplete)
			{
				activeShader.CompleteBinding(bufferId, indexBufferId);
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

		public unsafe void Flush()
		{
			if (buffer.Length == 0)
			{
				return;
			}

			// If no shader
			activeShader = activeShader ?? spriteShader;

			// This assumes that all 2D shaders will contain a uniform matrix called "mvp".
			activeShader.Apply();
			activeShader.SetUniform("mvp", mvp);

			if (primitiveRestartEnabled)
			{
				glEnable(GL_PRIMITIVE_RESTART);
			}
			else
			{
				glDisable(GL_PRIMITIVE_RESTART);
			}

			if (activeTexture != 0)
			{
				glActiveTexture(activeTexture);
				glBindTexture(GL_TEXTURE_2D, activeTexture);
			}

			fixed (byte* address = &buffer[0])
			{
				glBufferSubData(GL_ARRAY_BUFFER, 0, bufferSize, address);
			}

			fixed (ushort* address = &indexBuffer[0])
			{
				glBufferSubData(GL_ELEMENT_ARRAY_BUFFER, 0, sizeof(ushort) * indexCount, address);
			}

			glDrawElements(mode, indexCount, GL_UNSIGNED_SHORT, null);

			activeShader = null;
			activeTexture = 0;
			bufferSize = 0;
			indexCount = 0;
		}
	}
}
