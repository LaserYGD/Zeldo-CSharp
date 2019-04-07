using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Utility;
using GlmSharp;
using static Engine.GL;

namespace Engine.Core._2D
{
	public class Sprite : Component2D
	{
		// Each vertex contains position, texture coordinates, and color.
		private const int VertexSize = 5;

		private QuadSource source;
		private Bounds2D sourceRect;
		private vec2 position;
		private vec2 scale;
		private ivec2 origin;
		private Color color;

		private float rotation;
		private float[] data;

		private bool positionChanged;
		private bool sourceChanged;
		private bool colorChanged;

		public Sprite(string filename, Alignments alignment = Alignments.Center) :
			this (ContentCache.GetTexture(filename), alignment)
		{
		}

		public Sprite(QuadSource source, Alignments alignment = Alignments.Center)
		{
			this.source = source;

			data = new float[VertexSize * 4];
			origin = Utilities.ComputeOrigin(source.Width, source.Height, alignment);
			scale = vec2.Ones;

			// Calling the properties here (rather than assigning variables directly) sets the various changed booleans
			// to true.
			Position = vec2.Zero;
			Color = Color.White;
			SourceRect = null;
		}

		public override vec2 Position
		{
			get => position;
			set
			{
				position = value;
				positionChanged = true;
			}
		}

		public override vec2 Scale
		{
			get => scale;
			set
			{
				scale = value;
				positionChanged = true;
			}
		}

		public override float Rotation
		{
			get => rotation;
			set
			{
				rotation = value;
				positionChanged = true;
			}
		}

		public override Color Color
		{
			get => color;
			set
			{
				color = value;
				colorChanged = true;
			}
		}

		public Bounds2D SourceRect
		{
			get => sourceRect;
			set
			{
				sourceRect = value;
				sourceChanged = true;
			}
		}

		public void ScaleTo(ivec2 dimensions)
		{
			ScaleTo(dimensions.x, dimensions.y);
		}

		public void ScaleTo(int width, int height)
		{
		}

		private void RecomputePositionData()
		{
			int width = source.Width;
			int height = source.Height;

			vec2[] points =
			{
				new vec2(0, 0),
				new vec2(0, height),
				new vec2(width, 0),
				new vec2(width, height)
			};

			for (int i = 0; i < 4; i++)
			{
				vec2 p = points[i];
				p -= origin;
				p *= scale;
				points[i] = p;
			}

			if (rotation != 0)
			{
				mat2 rotationMatrix = new mat2(mat4.Rotate(rotation, vec3.UnitZ));

				for (int i = 0; i < 4; i++)
				{
					vec2 p = points[i];
					p = rotationMatrix * p;
					points[i] = p;
				}
			}

			for (int i = 0; i < 4; i++)
			{
				int start = i * VertexSize;

				vec2 p = points[i] + position;

				data[start] = p.x;
				data[start + 1] = p.y;
			}

			positionChanged = false;
		}

		public override void Draw(SpriteBatch sb)
		{
			if (positionChanged)
			{
				RecomputePositionData();
			}

			if (sourceChanged)
			{
				vec2[] coords = new vec2[4];

				if (sourceRect != null)
				{
					coords[0] = new vec2(sourceRect.Left, sourceRect.Top);
					coords[1] = new vec2(sourceRect.Left, sourceRect.Bottom);
					coords[2] = new vec2(sourceRect.Right, sourceRect.Top);
					coords[3] = new vec2(sourceRect.Right, sourceRect.Bottom);

					vec2 dimensions = Resolution.Dimensions;

					for (int i = 0; i < 4; i++)
					{
						coords[i] /= dimensions;
					}
				}
				else
				{
					coords[0] = vec2.Zero;
					coords[1] = vec2.UnitY;
					coords[2] = vec2.UnitX;
					coords[3] = vec2.Ones;
				}

				for (int i = 0; i < 4; i++)
				{
					vec2 value = coords[i];

					int start = i * VertexSize + 2;

					data[start] = value.x;
					data[start + 1] = value.y;
				}

				sourceChanged = false;
			}

			if (colorChanged)
			{
				float f = color.ToFloat();

				for (int i = 0; i < 4; i++)
				{
					data[i * VertexSize + 4] = f;
				}

				colorChanged = false;
			}
			
			sb.Mode = GL_TRIANGLE_STRIP;
			sb.BindTexture(source.Id);
			sb.Buffer(data);
		}	
	}
}
