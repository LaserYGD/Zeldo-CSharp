using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Exceptions;
using static Engine.GL;

namespace Engine.Shaders
{
	public class Shader
	{
		private uint program;
		private uint vao;
		private uint fragmentShader;
		private uint geometryShader;
		private uint vertexShader;

		private Dictionary<string, int> uniforms;

		public Shader()
		{
			uniforms = new Dictionary<string, int>();
		}

		public void Attach(ShaderTypes shaderType, string filename)
		{
			switch (shaderType)
			{
				case ShaderTypes.Fragment: fragmentShader = Load(filename, GL_VERTEX_SHADER); break;
				case ShaderTypes.Geometry: geometryShader = Load(filename, GL_GEOMETRY_SHADER); break;
				case ShaderTypes.Vertex: vertexShader = Load(filename, GL_VERTEX_SHADER); break;
			}
		}
	
		private unsafe uint Load(string filename, uint shaderType)
		{
			uint id = glCreateShader(shaderType);

			char[] source = File.ReadAllText("Content/Shaders/" + filename).ToCharArray();

			fixed (char* s = &source[0])
			{
				int length = source.Length;

				glShaderSource(id, 1, s, &length);
			}

			glCompileShader(id);

			int status;
			
			glGetShaderiv(id, GL_COMPILE_STATUS, &status);

			if (status == GL_FALSE)
			{
				int logSize;


				glGetShaderiv(id, GL_INFO_LOG_LENGTH, &logSize);

				char[] message = new char[logSize];

				fixed (char* messagePointer = &message[0])
				{
					glGetShaderInfoLog(id, (uint)logSize, null, messagePointer);
				}

				glDeleteShader(id);

				throw new ShaderException(ShaderStages.Compile, new string(message));
			}

			return id;
		}

		public unsafe void CreateProgram()
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

				char[] message = new char[logSize];

				fixed (char* messagePointer = &message[0])
				{
					glGetProgramInfoLog(program, (uint)logSize, null, messagePointer);
				}

				glDeleteProgram(program);
				DeleteShaders();

				throw new ShaderException(ShaderStages.Link, new string(message));
			}

			GetUniforms();
			DeleteShaders();

			fixed (uint* vaoAddress = &vao)
			{
				glGenVertexArrays(1, vaoAddress);
			}
		}

		private unsafe void GetUniforms()
		{
			int uniformCount;

			glGetProgramiv(program, GL_ACTIVE_UNIFORMS, &uniformCount);

			char[] name = new char[32];

			fixed (char* namePointer = &name[0])
			{
				for (int i = 0; i < uniformCount; i++)
				{
					uint length;
					uint type;
					int size;

					glGetActiveUniform(program, (uint)i, (uint)name.Length, &length, &size, &type, namePointer);
				}

				int location = glGetUniformLocation(program, namePointer);

				uniforms.Add(new string(name), location);
			}
		}

		private void DeleteShaders()
		{
			glDeleteShader(fragmentShader);
			glDeleteShader(geometryShader);
			glDeleteShader(vertexShader);
		}

		public void Apply()
		{
			glUseProgram(program);
		}
	}
}
