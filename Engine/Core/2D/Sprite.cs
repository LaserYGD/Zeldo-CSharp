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

namespace Engine.Core._2D
{
	public class Sprite : IPositionable2D, IRotatable, IColorable, IRenderable2D
	{
		// Each vertex contains position, texture coordinates, and color.
		private const int VertexSize = 5;

		private Texture texture;
		private vec2 position;
		private ivec2 origin;

		private float rotation;
		private float[] data;

		public Sprite(string filename, Alignments alignment = Alignments.Center) :
			this (ContentCache.GetTexture(filename), alignment)
		{
		}

		public Sprite(Texture texture, Alignments alignment = Alignments.Center)
		{
			this.texture = texture;

			data = new float[VertexSize * 4];
			origin = Utilities.ComputeOrigin(texture.Width, texture.Height, alignment);
			Position = vec2.Zero;
			Color = Color.White;
		}

		public vec2 Position
		{
			get => position;
			set
			{
				position = value;
				RecomputePositionData();
			}
		}

		public float Rotation
		{
			get => rotation;
			set
			{
				rotation = value;
				RecomputePositionData();
			}
		}

		public Color Color
		{
			get => throw new NotImplementedException();
			set
			{
				float f = value.ToFloat();

				for (int i = 0; i < 4; i++)
				{
					data[i * VertexSize + 4] = f;
				}
			}
		}

		private void RecomputePositionData()
		{
			uint width = texture.Width;
			uint height = texture.Height;

			vec2[] corners =
			{
				new vec2(0, 0), 
				new vec2(width, 0), 
				new vec2(width, height), 
				new vec2(0, height) 
			};

			mat2 rotationMatrix = new mat2(mat4.Rotate(rotation, vec3.UnitZ));

			for (int i = 0; i < 4; i++)
			{
				vec2 p = corners[i];
				p -= origin;
				p = rotationMatrix * p;
				p += position;

				int start = i * VertexSize;

				data[start] = p.x;
				data[start + 1] = p.y;
			}
		}

		public void Draw(SpriteBatch sb)
		{
		}
	}
}
