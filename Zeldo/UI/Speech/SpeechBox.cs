using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.UI;
using Engine.Utility;
using GlmSharp;

namespace Zeldo.UI.Speech
{
	public class SpeechBox : CanvasElement
	{
		private SpriteFont font;
		private List<SpriteText> lines;

		public SpeechBox()
		{
			font = ContentCache.GetFont("Default");
			lines = new List<SpriteText>();
			Bounds = new Bounds2D(400, 200);
			Anchor = Alignments.Bottom;
			Offset = new ivec2(0, 125);
			Centered = true;
		}

		public void Refresh(string value)
		{
			if (!Visible)
			{
				Visible = true;
			}

			string[] wrapped = Utilities.WrapLines(value, font, Bounds.Width);

			lines.Clear();

			vec2 position = Bounds.Location;

			foreach (var line in wrapped)
			{
				SpriteText spriteText = new SpriteText(font, line);
				spriteText.Position = position;

				lines.Add(spriteText);
				position.y += 20;
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			sb.Draw(Bounds, Color.White);
			lines.ForEach(l => l.Draw(sb));
		}
	}
}
