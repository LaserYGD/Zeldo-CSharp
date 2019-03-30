using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;

namespace Engine.Core
{
	public static class ContentCache
	{
		private static Dictionary<string, Texture> textures;

		static ContentCache()
		{
			textures = new Dictionary<string, Texture>();
		}

		public static Texture GetTexture(string filename)
		{
			if (textures.TryGetValue(filename, out Texture texture))
			{
				return texture;
			}

			texture = Texture.Load(filename);
			textures.Add(filename, texture);

			return texture;
		}
	}
}
