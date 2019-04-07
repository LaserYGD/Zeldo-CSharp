using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;

namespace Engine
{
	public static class ContentCache
	{
		private static Dictionary<string, SpriteFont> fonts;
		private static Dictionary<string, Texture> textures;

		static ContentCache()
		{
			fonts = new Dictionary<string, SpriteFont>();
			textures = new Dictionary<string, Texture>();
		}

		public static SpriteFont GetFont(string name)
		{
			if (!fonts.TryGetValue(name, out SpriteFont font))
			{
				font = SpriteFont.Load(name);
				fonts.Add(name, font);
			}

			return font;
		}

		public static Texture GetTexture(string filename, string folder = "Textures/")
		{
			if (!textures.TryGetValue(filename, out Texture texture))
			{
				texture = Texture.Load(filename, folder);
				textures.Add(filename, texture);
			}

			return texture;
		}
	}
}
