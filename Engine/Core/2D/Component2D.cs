using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using GlmSharp;
using static Engine.GL;

namespace Engine.Core._2D
{
	public abstract class Component2D : IPositionable2D, IRotatable, IScalable2D, IColorable, IRenderable2D
	{
		// Each vertex contains position, texture coordinates, and color.
		protected const int VertexSize = 5;
		protected const int QuadSize = VertexSize * 4;

		protected vec2 position;
		protected vec2 scale;
		protected ivec2 origin;
		protected Color color;

		protected float rotation;
		protected float[] data;

		protected Alignments alignment;

		protected bool positionChanged;
		protected bool sourceChanged;
		protected bool colorChanged;

		protected Component2D()
		{
			scale = vec2.Ones;
			color = Color.White;
			positionChanged = true;
			colorChanged = true;
		}

		public vec2 Position
		{
			get => position;
			set
			{
				position = value;
				positionChanged = true;
			}
		}

		public vec2 Scale
		{
			get => scale;
			set
			{
				scale = value;
				positionChanged = true;
			}
		}

		public float Rotation
		{
			get => rotation;
			set
			{
				rotation = value;
				positionChanged = true;
			}
		}

		public Color Color
		{
			get => color;
			set
			{
				color = value;
				colorChanged = true;
			}
		}

		protected abstract void RecomputePositionData();
		protected abstract void RecomputeSourceData();

		private void RecomputeColorData()
		{
			// The data array can be null for strings with no value.
			if (data == null)
			{
				return;
			}

			float f = color.ToFloat();

			for (int i = 0; i < data.Length / VertexSize; i++)
			{
				data[i * VertexSize + 4] = f;
			}
		}

		public abstract void Draw(SpriteBatch sb);

		protected void Draw(SpriteBatch sb, uint textureId, float[] data)
		{
			if (positionChanged)
			{
				RecomputePositionData();
				positionChanged = false;
			}

			if (sourceChanged)
			{
				RecomputeSourceData();
				sourceChanged = false;
			}

			if (colorChanged)
			{
				RecomputeColorData();
				colorChanged = false;
			}

			sb.Mode = GL_TRIANGLE_STRIP;
			sb.BindTexture(textureId);

			// Strings need to buffer each character individually in order to add the primitive restart index each
			// time.
			int quads = data.Length / QuadSize;

			ushort[] indices =
			{
				0, 1, 2, 3
			};

			for (int i = 0; i < quads; i++)
			{
				sb.Buffer(data, indices, i * QuadSize, QuadSize);
			}
		}
	}
}
