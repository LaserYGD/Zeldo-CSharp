using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Utility;
using GlmSharp;

namespace Engine.Core._2D
{
	public class Sprite : Component2D
	{
		private QuadSource source;
		private Bounds2D sourceRect;

		public Sprite(string filename, Alignments alignment = Alignments.Center) :
			this (ContentCache.GetTexture(filename), alignment)
		{
		}

		public Sprite(QuadSource source, Alignments alignment = Alignments.Center)
		{
			this.source = source;

			data = new float[QuadSize];

			// Calling the property here sets the sourceChanged flag to true and recomputes the origin.
			SourceRect = null;
		}

		public Bounds2D SourceRect
		{
			get => sourceRect;
			set
			{
				sourceRect = value;
				sourceChanged = true;

				int width;
				int height;

				if (sourceRect == null)
				{
					width = source.Width;
					height = source.Height;
				}
				else
				{
					width = sourceRect.Width;
					height = sourceRect.Height;
				}

				origin = Utilities.ComputeOrigin(width, height, alignment);
			}
		}

		public void ScaleTo(ivec2 dimensions)
		{
			ScaleTo(dimensions.x, dimensions.y);
		}

		public void ScaleTo(int width, int height)
		{
		}

		protected override void RecomputePositionData()
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
		}

		protected override void RecomputeSourceData()
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
		}

		public override void Draw(SpriteBatch sb)
		{
			Draw(sb, source.Id, data);
		}	
	}
}
