using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSGL.OpenGL;

namespace Engine.Graphics
{
	public class Texture
	{
		public static unsafe Texture Load(string filename)
		{
			Bitmap image = new Bitmap("Textures/" + filename);

			uint id = 0;
			int width = image.Width;
			int height = image.Height;
			int[] data = new int[width * height];

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					data[i * width + j] = image.GetPixel(j, i).ToArgb();
				}
			}

			fixed (int* dataPointer = &data[0])
			{
				glGenTextures(1, ref id);
				glBindTexture(GL_TEXTURE_2D, id);
				glTexImage2D(GL_TEXTURE_2D, 0, (int)GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE,
					new IntPtr(dataPointer));
			}

			return new Texture(id, width, height);
		}

		private Texture(uint id, int width, int height)
		{
			Id = id;
			Width = width;
			Height = height;
		}

		public uint Id { get; }
		public int Width { get; }
		public int Height { get; }
	}
}
