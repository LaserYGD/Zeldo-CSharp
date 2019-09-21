using System.Collections.Generic;
using Engine;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.UI;
using Engine.Utility;
using GlmSharp;

namespace Zeldo.UI
{
	public class StatisticsDisplay : CanvasElement
	{
		private readonly int lineSpacing = Properties.GetInt("debug.line.spacing");

		private List<SpriteText> lines;
		private SpriteFont font;

		public StatisticsDisplay()
		{
			lines = new List<SpriteText>();
			font = ContentCache.GetFont("Debug");
		}

		public override ivec2 Location
		{
			get => base.Location;
			set
			{
				Utilities.PositionItems(lines, value, new vec2(0, font.Size + lineSpacing));

				base.Location = value;
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			// Locking prevents the draw calls below from wastefully upping stats (which are reset at the end of this
			// function anyway).
			Statistics.Lock();

			var results = Statistics.Enumerate();
			var delta = results.Length - lines.Count;

			// For the time being, it's assumed that debug keys will never be removed.
			if (delta > 0)
			{
				for (int i = 0; i < delta; i++)
				{
					lines.Add(new SpriteText(font));
				}

				// This recomputes positions (since new lines were added). It's technically wasteful to reposition
				// earlier lines too, but it's a negligible problem.
				Location = Location;
			}

			for (int i = 0; i < results.Length; i++)
			{
				var line = lines[i];
				var result = results[i];

				line.Value = $"{result.Key}: {result.Value}";
				line.Draw(sb);
			}

			Statistics.Reset();
		}
	}
}
