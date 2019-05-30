using System.Drawing;
using Engine.Core._2D;
using static Engine.GL;

namespace Engine.Graphics
{
	public class Texture : QuadSource
	{
		public static unsafe Texture Load(string filename, string folder)
		{
			LoadData(folder + filename, out int width, out int height, out int[] data);

			uint id = 0;

			glGenTextures(1, &id);

			// Setting the data also binds the texture.
			Texture texture = new Texture(id, width, height);
			texture.Data = data;

			SetDefaultParameters();

			return texture;
		}

		// This function is useful externally to load texture data without using OpenGL calls. Useful when a texture
		// is used for purposes other than direct rendering.
		public static void LoadData(string path, out int width, out int height, out int[] data)
		{
			Bitmap image = new Bitmap("Content/" + path);
			
			width = image.Width;
			height = image.Height;

			data = new int[width * height];

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					data[i * width + j] = image.GetPixel(j, i).ToRgba();
				}
			}
		}

		private static void SetDefaultParameters()
		{
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
		}

		// This constructor is useful when generating texture data dynamically at runtime.
		public unsafe Texture()
		{
			uint id = 0;

			glGenTextures(1, &id);
			SetDefaultParameters();

			Id = id;
		}

		private Texture(uint id, int width, int height) : base(id, width, height)
		{
		}

		public unsafe int[] Data
		{
			set
			{
				glBindTexture(GL_TEXTURE_2D, Id);

				fixed (int* dataPointer = &value[0])
				{
					glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, (uint)Width, (uint)Height, 0, GL_RGBA, GL_UNSIGNED_BYTE,
						dataPointer);
				}
			}
		}
	}
}
