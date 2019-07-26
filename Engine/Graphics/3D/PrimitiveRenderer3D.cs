using System;
using Engine.Core;
using Engine.Shaders;
using Engine.Shapes._2D;
using Engine.Shapes._3D;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics._3D
{
	public class PrimitiveRenderer3D : IDisposable
	{
		private Camera3D camera;
		private Shader shader;
		private PrimitiveBuffer buffer;

		private uint bufferId;
		private uint indexId;
		private uint mode;

		public PrimitiveRenderer3D(Camera3D camera, int bufferSize, int indexSize)
		{
			this.camera = camera;

			buffer = new PrimitiveBuffer(bufferSize, indexSize);

			GLUtilities.AllocateBuffers(bufferSize, indexSize, out bufferId, out indexId, GL_DYNAMIC_DRAW);

			shader = new Shader(bufferId, indexId);
			shader.Attach(ShaderTypes.Vertex, "Primitives3D.vert");
			shader.Attach(ShaderTypes.Fragment, "Primitives.frag");
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.AddAttribute<byte>(4, GL_UNSIGNED_BYTE, ShaderAttributeFlags.IsNormalized);
			shader.Initialize();
		}

		public void Dispose()
		{
			shader.Dispose();

			GLUtilities.DeleteBuffers(bufferId, indexId);
		}

		public void Draw(Arc arc, float y, Color color, int segments)
		{
			vec2 center = arc.Position;
			vec3[] points = new vec3[segments + 2];

			float start = arc.Angle - arc.Spread / 2;
			float increment = arc.Spread / segments;

			for (int i = 0; i <= segments; i++)
			{
				vec2 p = center + Utilities.Direction(start + increment * i) * arc.Radius;

				points[i] = new vec3(p.x, y, p.y);
			}

			points[points.Length - 1] = new vec3(center.x, y, center.y);

			Buffer(points, color, GL_LINE_LOOP);
		}

		public void Draw(Box box, Color color)
		{
			vec3 halfSize = new vec3(box.Width, box.Height, box.Depth) / 2;
			vec3 min = -halfSize;
			vec3 max = halfSize;
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
					points[i] = orientation * points[i];
				}
			}

			for (int i = 0; i < points.Length; i++)
			{
				points[i] += box.Position;
			}

			ushort[] indices =
			{
				0, 1, 1, 2, 2, 3, 3, 0,
				4, 5, 5, 6, 6, 7, 7, 4,
				0, 6, 1, 7, 2, 4, 3, 5
			};

			Buffer(points, color, GL_LINES, indices);
		}

		public void Draw(float radius, vec3 center, quat orientation, Color color, int segments)
		{
			vec3[] points = new vec3[segments];

			float increment = Constants.TwoPi / segments;

			for (int i = 0; i < points.Length; i++)
			{
				vec2 p = Utilities.Direction(increment * i) * radius;

				points[i] = orientation * new vec3(p.x, 0, p.y) + center;
			}

			Buffer(points, color, GL_LINE_LOOP);
		}

		public void Draw(Line3D line, Color color)
		{
			DrawLine(line.P1, line.P2, color);
		}

		public void DrawLine(vec3 p1, vec3 p2, Color color)
		{
			vec3[] points = { p1, p2 };

			Buffer(points, color, GL_LINES);
		}

		public void DrawTriangle(vec3 p0, vec3 p1, vec3 p2, Color color)
		{
			DrawTriangle(new [] { p0, p1, p2 }, color);
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

		private void Buffer(vec3[] points, Color color, uint mode, ushort[] indices = null)
		{
			if (this.mode != mode)
			{
				Flush();

				this.mode = mode;
		
				buffer.Mode = mode;
			}

			float[] data = GetData(points, color);

			// If the index array is null, it's assumed that indices can be added sequentially.
			if (indices == null)
			{
				indices = new ushort[points.Length];

				for (int i = 0; i < indices.Length; i++)
				{
					indices[i] = (ushort)i;
				}
			}

			buffer.Buffer(data, indices);
		}

		public unsafe void Flush()
		{
			if (buffer.Size == 0)
			{
				return;
			}

			shader.Apply();
			shader.SetUniform("mvp", camera.ViewProjection);

			glDrawElements(mode, buffer.Flush(), GL_UNSIGNED_SHORT, null);
		}
	}
}
