using System.Linq;
using Engine;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.UI;
using Engine.Utility;
using GlmSharp;

namespace Zeldo.UI
{
	public class PlayerDebugView : CanvasElement
	{
		private SpriteFont font;
		private SpriteText[] lines;

		public PlayerDebugView()
		{
			font = ContentCache.GetFont("Debug");
		}

		public string[] Lines
		{
			set
			{
				lines = value.Select(l => new SpriteText(font, l)).ToArray();

				Utilities.PositionItems(lines, new vec2(20), new vec2(0, 20));
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			if (lines == null)
			{
				return;
			}

			foreach (var line in lines)
			{
				line.Draw(sb);
			}
		}
	}
}
