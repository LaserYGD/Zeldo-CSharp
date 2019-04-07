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

		public static SpriteFont GetFont(string filename)
		{
			if (!fonts.TryGetValue(filename, out SpriteFont font))
			{
				font = SpriteFont.Load(filename);
				fonts.Add(filename, font);
			}

			return font;
		}

		public static Texture GetTexture(string name)
		{
			if (!textures.TryGetValue(name, out Texture texture))
			{
				texture = Texture.Load(name);
				textures.Add(name, texture);
			}

			return texture;
		}
	}
}
