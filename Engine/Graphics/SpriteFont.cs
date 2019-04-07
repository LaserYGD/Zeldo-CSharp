using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;

namespace Engine.Graphics
{
	public class SpriteFont
	{
		// This covers common English characters, numbers, and punctuation.
		private const int CharacterRange = 127;

		public static SpriteFont Load(string name)
		{
			const string Path = "Content/Fonts/";

			Texture texture = ContentCache.GetTexture(Path + name + "_0.png");

			string[] lines = File.ReadAllLines(Path + name + ".fnt");
			string first = lines[0];

			// Size is in the form "size=[value]" (without the square brackets);
			int index1 = first.IndexOf("size") + 5;
			int index2 = first.IndexOf(' ', index1);
			int size = int.Parse(first.Substring(index1, index2 - index1));

			// Character count (the fourth line) looks like "chars count=[value]".
			int count = int.Parse(lines[3].Substring(12));

			Glyph[] glyphs = new Glyph[CharacterRange];

			for (int i = 0; i < count; i++)
			{
				int ParseValue(string s)
				{
					int index = s.IndexOf('=') + 1;

					return int.Parse(s.Substring(index));
				}

				string line = lines[i + 4];
				string[] tokens = line.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);

				// The first token is just "char", so it can be ignored.
				int id = ParseValue(tokens[1]);
				int x = ParseValue(tokens[2]);
				int y = ParseValue(tokens[3]);
				int width = ParseValue(tokens[4]);
				int height = ParseValue(tokens[5]);
				int offsetX = ParseValue(tokens[6]);
				int offsetY = ParseValue(tokens[7]);
				int advance = ParseValue(tokens[8]);
				int w = texture.Width;
				int h = texture.Height;

				vec2[] source =
				{
					new vec2(x, y),
					new vec2(x, y + h),
					new vec2(x + w, y),
					new vec2(x + w, y + h) 
				};

				for (int j = 0; j < 4; j++)
				{
					source[j] /= new vec2(w, h);
				}

				glyphs[i] = new Glyph(width, height, advance, new ivec2(offsetX, offsetY), source);
			}

			return new SpriteFont(glyphs, texture.Id);
		}

		private SpriteFont(Glyph[] glyphs, uint textureId)
		{
			Glyphs = glyphs;
			TextureId = textureId;
		}

		public Glyph[] Glyphs { get; }

		public uint TextureId { get; }

		public ivec2 Measure(string value)
		{
			return ivec2.Zero;
		}

		public ivec2 MeasureLiteral(string value, out ivec2 offset)
		{
			offset = ivec2.Zero;

			return ivec2.Zero;
		}
	}
}
