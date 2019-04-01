﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core._2D;
using static Engine.GL;

namespace Engine.Graphics
{
	public class Texture : QuadSource
	{
		public static unsafe Texture Load(string filename)
		{
			Bitmap image = new Bitmap("Content/Textures/" + filename);

			uint id = 0;
			uint width = (uint)image.Width;
			uint height = (uint)image.Height;
			int[] data = new int[width * height];

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					data[i * width + j] = image.GetPixel(j, i).ToArgb();
				}
			}

			glGenTextures(1, &id);
			glBindTexture(GL_TEXTURE_2D, id);

			fixed (int* dataPointer = &data[0])
			{
				glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE,
					dataPointer);
			}

			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);

			return new Texture(id, width, height);
		}

		private Texture(uint id, uint width, uint height) : base(id, width, height)
		{
		}
	}
}
