using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core;
using Engine.Interfaces._3D;
using Engine.Shaders;
using Engine.Shapes._3D;
using Engine.View;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics
{
	public class PrimitiveRenderer3D
	{
		private Shader primitiveShader;
		private PrimitiveBuffer buffer;

		private uint bufferId;
		private uint indexBufferId;

		public PrimitiveRenderer3D()
		{
			const int BufferCapacity = 2048;
			const int IndexCapacity = 256;

			buffer = new PrimitiveBuffer(BufferCapacity, IndexCapacity);

			GLUtilities.AllocateBuffers(BufferCapacity, IndexCapacity, out bufferId, out indexBufferId);

			primitiveShader = new Shader();
			primitiveShader.Attach(ShaderTypes.Vertex, "Primitives3D.vert");
			primitiveShader.Attach(ShaderTypes.Fragment, "Primitives.frag");
			primitiveShader.CreateProgram();
			primitiveShader.AddAttribute<float>(3, GL_FLOAT);
			primitiveShader.AddAttribute<byte>(4, GL_UNSIGNED_BYTE, true);
			primitiveShader.Bind(bufferId, indexBufferId);
		}

		public void Draw(Box box, Color color)
		{
			vec3 center = box.Position;
			vec3 halfSize = new vec3(box.Width, box.Height, box.Depth) / 2;
			vec3 min = center - halfSize;
			vec3 max = center + halfSize;
			vec3[] points =
			{
				min,
				new vec3(min.x, min.y, max.z),
				new vec3(min.x, max.y, max.z),
				new vec3(min.x, max.y, min.z),
				max,
				new vec3(max.x, max.y, min.z),
				new vec3(max.x, min.y, min.z),
				new vec3(max.x, min.y, max.z) 
			};

			quat orientation = box.Orientation;

			if (orientation != quat.Identity)
			{
				for (int i = 0; i < points.Length; i++)
				{
					points[i] *= orientation;
				}
			}

			float[] data = GetData(points, color);

			ushort[] indices =
			{
				0, 1, 1, 2, 2, 3, 3, 0,
				4, 5, 5, 6, 6, 7, 7, 4,
				0, 6, 1, 7, 2, 4, 3, 5
			};

			buffer.Buffer(data, indices);
		}

		public void DrawTriangle(vec3[] points, Color color)
		{
			float[] data = GetData(points, color);
			ushort[] indices = { 0, 1, 2, 0 };

			buffer.Buffer(data, indices);
		}

		private float[] GetData(vec3[] points, Color color)
		{
			float f = color.ToFloat();
			float[] data = new float[points.Length * 4];

			for (int i = 0; i < points.Length; i++)
			{
				int start = i * 4;

				vec3 p = points[i];

				data[start] = p.x;
				data[start + 1] = p.y;
				data[start + 2] = p.z;
				data[start + 3] = f;
			}

			return data;
		}

		public unsafe void Flush(Camera3D camera)
		{
			if (buffer.Size == 0)
			{
				return;
			}

			primitiveShader.Apply();
			primitiveShader.SetUniform("mvp", camera.ViewProjection);

			glDrawElements(GL_LINES, buffer.Flush(), GL_UNSIGNED_SHORT, null);
			//glDrawElements(GL_LINE_LOOP, buffer.Flush(), GL_UNSIGNED_SHORT, null);
		}
	}
}
