using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

		public static Mesh GetMesh(string filename, bool shouldCache = true)
		{
			Debug.Assert(!string.IsNullOrEmpty(filename), "Mesh filename can't be empty or null.");

			if (!meshes.TryGetValue(filename, out Mesh mesh))
			{
				mesh = Mesh.Load(filename);

				if (shouldCache)
				{
					meshes.Add(filename, mesh);
				}
			}

			return mesh;
		}

		public static void ClearMesh(Mesh mesh)
		{
			Debug.Assert(meshes.ContainsValue(mesh), "Cache doesn't contain the given mesh.");

			meshes.Remove(meshes.First(pair => pair.Value == mesh).Key);
		}

		public static SpriteFont GetFont(string name, bool shouldCache = true)
		{
			Debug.Assert(!string.IsNullOrEmpty(name), "Font name can't be empty or null.");

			if (!fonts.TryGetValue(name, out SpriteFont font))
			{
				font = SpriteFont.Load(name, shouldCache);

				if (shouldCache)
				{
					fonts.Add(name, font);
				}
			}

			return font;
		}

		public static void ClearFont(SpriteFont font)
		{
			Debug.Assert(fonts.ContainsValue(font), "Cache doesn't contain the given font.");

			fonts.Remove(fonts.First(pair => pair.Value == font).Key);
		}

		public static Texture GetTexture(string filename, bool shouldStoreData = false, bool shouldCache = true,
			string folder = "Textures/")
		{
			Debug.Assert(!string.IsNullOrEmpty(filename), "Texture filename can't be empty or null.");
			Debug.Assert(!string.IsNullOrEmpty(folder), "Texture folder can't be empty or null.");

			if (!textures.TryGetValue(filename, out Texture texture))
			{
				texture = Texture.Load(filename, folder, shouldStoreData);

				// It's possible for this flag to be false even though the texture is already cached (i.e. was
				// previously loaded and cached). This scenario shouldn't occur in practice, but also doesn't really
				// matter if it does.
				if (shouldCache)
				{
					textures.Add(filename, texture);
				}
			}

			return texture;
		}

		public static void ClearTexture(Texture texture)
		{
			Debug.Assert(textures.ContainsValue(texture), "Cache doesn't contain the given texture.");

			textures.Remove(textures.First(pair => pair.Value == texture).Key);
		}
	}
}
