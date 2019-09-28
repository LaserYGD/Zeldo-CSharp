using System.Collections.Generic;
using System.Diagnostics;
using Engine.Graphics;
using Engine.Graphics._2D;
using Engine.Graphics._3D;

namespace Engine
{
	public static class ContentCache
	{
		private static Dictionary<string, SpriteFont> fonts;
		private static Dictionary<string, Mesh> meshes;
		private static Dictionary<string, Texture> textures;

		static ContentCache()
		{
			fonts = new Dictionary<string, SpriteFont>();
			meshes = new Dictionary<string, Mesh>();
			textures = new Dictionary<string, Texture>();
		}

		public static Mesh GetMesh(string filename)
		{
			Debug.Assert(!string.IsNullOrEmpty(filename), "Mesh filename can't be empty or null.");

			if (!meshes.TryGetValue(filename, out Mesh mesh))
			{
				mesh = Mesh.Load(filename);
				meshes.Add(filename, mesh);
			}

			return mesh;
		}

		public static SpriteFont GetFont(string name)
		{
			Debug.Assert(!string.IsNullOrEmpty(name), "Font name can't be empty or null.");

			if (!fonts.TryGetValue(name, out SpriteFont font))
			{
				font = SpriteFont.Load(name);
				fonts.Add(name, font);
			}

			return font;
		}

		public static Texture GetTexture(string filename, bool shouldStoreData = false, string folder = "Textures/")
		{
			Debug.Assert(!string.IsNullOrEmpty(filename), "Texture filename can't be empty or null.");
			Debug.Assert(!string.IsNullOrEmpty(folder), "Texture folder can't be empty or null.");

			if (!textures.TryGetValue(filename, out Texture texture))
			{
				texture = Texture.Load(filename, folder, shouldStoreData);
				textures.Add(filename, texture);
			}

			return texture;
		}
	}
}
