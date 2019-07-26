using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Engine.Exceptions;
using GlmSharp;
using static Engine.GL;

namespace Engine.Shaders
{
	public class Shader : IDisposable
	{
		private uint program;
		private uint vao;
		private uint fragmentShader;
		private uint geometryShader;
		private uint vertexShader;

		private uint bufferId;
		private uint indexId;

		private List<ShaderAttribute> attributes;
		private Dictionary<string, int> uniforms;

		public Shader() : this(0, 0)
		{
		}

		public Shader(uint bufferId) : this(bufferId, 0)
		{
		}

		public Shader(uint bufferId, uint indexId)
		{
			this.bufferId = bufferId;
			this.indexId = indexId;

			attributes = new List<ShaderAttribute>();
			uniforms = new Dictionary<string, int>();
		}

		public bool IsBindingComplete { get; private set; }
		public bool IsDisposed { get; private set; }

		public uint Stride { get; private set; }

		public void Attach(ShaderTypes shaderType, string filename)
		{
			switch (shaderType)
			{
				case ShaderTypes.Vertex: vertexShader = Load(filename, GL_VERTEX_SHADER); break;
				case ShaderTypes.Geometry: geometryShader = Load(filename, GL_GEOMETRY_SHADER); break;
				case ShaderTypes.Fragment: fragmentShader = Load(filename, GL_FRAGMENT_SHADER); break;
			}
		}
	
		private unsafe uint Load(string filename, uint shaderType)
		{
			uint id = glCreateShader(shaderType);

			string source = File.ReadAllText("Content/Shaders/" + filename);
			
			int length = source.Length;

			glShaderSource(id, 1, new [] { source }, &length);
			glCompileShader(id);

			int status;
			
			glGetShaderiv(id, GL_COMPILE_STATUS, &status);

			if (status == GL_FALSE)
			{
				int logSize;

				glGetShaderiv(id, GL_INFO_LOG_LENGTH, &logSize);

				byte[] message = new byte[logSize];

				fixed (byte* messagePointer = &message[0])
				{
					glGetShaderInfoLog(id, (uint)logSize, null, messagePointer);
				}

				glDeleteShader(id);

				throw new ShaderException(ShaderStages.Compile, Encoding.Default.GetString(message));
			}

			return id;
		}

		public unsafe void Initialize()
		{
			program = glCreateProgram();

			glAttachShader(program, vertexShader);
			glAttachShader(program, fragmentShader);

			if (geometryShader != 0)
			{
				glAttachShader(program, geometryShader);
			}

			glLinkProgram(program);

			int status;

			glGetProgramiv(program, GL_LINK_STATUS, &status);

			if (status == GL_FALSE)
			{
				int logSize;

				glGetProgramiv(program, GL_INFO_LOG_LENGTH, &logSize);

				byte[] message = new byte[logSize];

				fixed (byte* messagePointer = &message[0])
				{
					glGetProgramInfoLog(program, (uint)logSize, null, messagePointer);
				}

				glDeleteProgram(program);
				DeleteShaders();

				// See https://stackoverflow.com/a/11654597/7281613.
				throw new ShaderException(ShaderStages.Link, Encoding.Default.GetString(message));
			}

			GetUniforms();
			DeleteShaders();

			if (bufferId > 0)
			{
				GenerateVao();
			}
		}

		private unsafe void GetUniforms()
		{
			int uniformCount;

			glGetProgramiv(program, GL_ACTIVE_UNIFORMS, &uniformCount);

			byte[] bytes = new byte[64];

			fixed (byte* address = &bytes[0])
			{
				for (int i = 0; i < uniformCount; i++)
				{
					uint length;
					uint type;
					int size;

					glGetActiveUniform(program, (uint)i, (uint)bytes.Length, &length, &size, &type, address);

					int location = glGetUniformLocation(program, address);

					string name = Encoding.Default.GetString(bytes).Substring(0, (int)length);

					uniforms.Add(name, location);
				}
			}
		}

		private void DeleteShaders()
		{
			glDeleteShader(vertexShader);
			glDeleteShader(geometryShader);
			glDeleteShader(fragmentShader);
		}

		public void AddAttribute<T>(uint count, uint type, ShaderAttributeFlags flags = ShaderAttributeFlags.None,
			uint padding = 0)
		{
			attributes.Add(new ShaderAttribute(count, type, Stride, flags));

			// Padding is given in bytes directly (so that the padding can encompass data of multiple types).
			Stride += (uint)Marshal.SizeOf<T>() * count + padding;
		}

		public void Bind(uint bufferId, uint indexId)
		{
			this.bufferId = bufferId;
			this.indexId = indexId;

			GenerateVao();
		}

		private unsafe void GenerateVao()
		{
			fixed (uint* address = &vao)
			{
				glGenVertexArrays(1, address);
			}

			glBindVertexArray(vao);
			glBindBuffer(GL_ARRAY_BUFFER, bufferId);

			if (indexId > 0)
			{
				glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);
			}

			for (int i = 0; i < attributes.Count; i++)
			{
				ShaderAttribute attribute = attributes[i];

				uint index = (uint)i;

				if (attribute.IsInteger)
				{
					glVertexAttribIPointer(index, (int)attribute.Count, attribute.Type, Stride,
						(void*)attribute.Offset);
				}
				else
				{
					glVertexAttribPointer(index, (int)attribute.Count, attribute.Type, attribute.IsNormalized, Stride,
						(void*)attribute.Offset);
				}

				glEnableVertexAttribArray(index);
			}

			// This assumes that shaders won't be bound twice.
			attributes = null;
			IsBindingComplete = true;
		}

		public unsafe void Dispose()
		{
			// Sprites can have shaders applied. Those sprites may or may not own the shader, though. This check
			// prevents duplicate disposal if the shader is disposed from multiple places.
			if (IsDisposed)
			{
				return;
			}

			glDeleteProgram(program);

			fixed (uint* address = &vao)
			{
				glDeleteVertexArrays(1, address);
			}

			IsDisposed = true;
		}

		public void Use()
		{
			glUseProgram(program);
		}

		public void Apply()
		{
			if (vao == 0)
			{
				throw new ShaderException(ShaderStages.Apply, "The shader's VAO was zero when applied. This likely " +
					"means the shader was never bound.");
			}

			glUseProgram(program);
			glBindVertexArray(vao);
			glBindBuffer(GL_ARRAY_BUFFER, bufferId);
			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);
		}

		public void SetUniform(string name, int value)
		{
			glUniform1i(uniforms[name], value);
		}

		public void SetUniform(string name, float value)
		{
			glUniform1f(uniforms[name], value);
		}

		public void SetUniform(string name, vec3 value)
		{
			glUniform3f(uniforms[name], value.x, value.y, value.z);
		}

		public void SetUniform(string name, vec4 value)
		{
			glUniform4f(uniforms[name], value.r, value.g, value.b, value.a);
		}

		public unsafe void SetUniform(string name, vec4[] values)
		{
			fixed (float* address = &values[0].x)
			{
				glUniform4fv(uniforms[name], (uint)values.Length, address);
			}
		}

		public unsafe void SetUniform(string name, mat4 value)
		{
			float[] values = value.Values1D;

			fixed (float* address = &values[0])
			{
				glUniformMatrix4fv(uniforms[name], 1, false, address);
			}
		}

		public unsafe void SetUniform(string name, mat4[] values)
		{
			float[] floats = new float[values.Length * 16];

			for (int i = 0; i < values.Length; i++)
			{
				int start = i * 16;

				var array = values[i].Values1D;

				for (int j = 0; j < 16; j++)
				{
					floats[start + j] = array[j];
				}
			}

			fixed (float* address = &floats[0])
			{
				glUniformMatrix4fv(uniforms[name], (uint)values.Length, false, address);
			}
		}
	}
}
