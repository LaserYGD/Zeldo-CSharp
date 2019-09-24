using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.UI;

namespace Zeldo.UI
{
	public class DebugView : CanvasElement
	{
		private readonly int lineSpacing = Properties.GetInt("debug.line.spacing");
		private readonly int blockSpacing = Properties.GetInt("debug.block.spacing");

		private SpriteFont font;
		private Dictionary<string, List<string>> rawGroups;
		private Dictionary<string, List<SpriteText>> textGroups;
		private List<string> groupOrder;

		public DebugView()
		{
			font = ContentCache.GetFont("Debug");
			rawGroups = new Dictionary<string, List<string>>();
			textGroups = new Dictionary<string, List<SpriteText>>();
			groupOrder = new List<string>();
		}

		// This is used to highlight group names in a different color (or possibly other noteworthy fields in the
		// future).
		public Color Highlight { get; set; } = new Color(255, 215, 0);

		public void Add(string group, string value)
		{
			GetGroup(group).Add(value);
		}

		public List<string> GetGroup(string group)
		{
			if (!rawGroups.TryGetValue(group, out var list))
			{
				// The first entry in each block is the name of the group.
				var groupText = new SpriteText(font, group);
				groupText.Y = Location.y;
				groupText.Color = Highlight;

				// The first group name will never have its X position changed by other blocks.
				if (rawGroups.Count == 0)
				{
					groupText.X = Location.x;
				}

				var textList = new List<SpriteText>();
				textList.Add(groupText);

				list = new List<string>();
				rawGroups.Add(group, list);
				textGroups.Add(group, textList);
				groupOrder.Add(group);
			}

			return list;
		}

		public override void Draw(SpriteBatch sb)
		{
			// This means that no debug lines were added at all (to any group).
			if (groupOrder.Count == 0)
			{
				return;
			}

			RefreshGroups();

			// Using group order ensures that group blocks are displayed in the order they were added.
			foreach (string group in groupOrder)
			{
				textGroups[group].ForEach(t => t.Draw(sb));
				rawGroups[group].Clear();
			}
		}

		private void RefreshGroups()
		{
			// The X position of each block is updated dynamically each frame based on the strings added to that group.
			// To avoid jitter, it's best to use fixed widths on numbers (especially decimal points on floats).
			int nextX = Location.x;

			for (int i = 0; i < groupOrder.Count; i++)
			{
				var group = groupOrder[i];
				var rawList = rawGroups[group];
				var textList = textGroups[group];

				int rawCount = rawList.Count;
				int entryCount = textList.Count - 1;

				// Raw entries for each group are cleared each frame, which means that extra text objects need to be
				// removed as well (with the exception of the group name, which isn't removed). The goal here is to
				// make the debug view feel more responsive and immediate to changes in the game.
				if (rawCount == 0 && entryCount > 0)
				{
					for (int j = entryCount; j >= 0; j--)
					{
						textList[j].Dispose();
						textList.RemoveAt(j);
					}
				}

				// If there are no entries for the current group, nothing else needs to be done (past recording the
				// text width of the group name).
				if (rawCount == 0)
				{
					nextX += font.Measure(group).x;

					continue;
				}

				// If the counts are equal, text objects need to have their values updated.
				if (rawCount == entryCount)
				{
					for (int j = 0; j < entryCount; j++)
					{
						textList[j + 1].Value = rawList[j];
					}
				}
				// If there are fewer raw entries than text objects, relevant objects have their values updated and
				// extras are removed.
				else if (rawCount < entryCount)
				{
					for (int j = 0; j < rawCount; j++)
					{
						textList[j + 1].Value = rawList[j];
					}

					for (int j = entryCount; j > rawCount; j--)
					{
						textList[j].Dispose();
						textList.RemoveAt(j);
					}
				}
				else
				{
					// If there are fewer text objects than raw entries, existing objects have their values update and
					// new text objects are created to match the extra strings.
					for (int j = 0; j < entryCount; j++)
					{
						textList[j + 1].Value = rawList[j];
					}

					for (int j = entryCount; j < rawCount; j++)
					{
						// This is used to compute Y position. Note that lines are positioned such that there's an
						// empty line between the group and the first entry.
						int index = j - entryCount + 2;

						var text = new SpriteText(font, rawList[j]);
						text.Y = Location.y + index * font.Size + Math.Max(index - 1, 0) * lineSpacing;

						// Just like the group name, lines in the first block will never change their X position.
						if (i == 0)
						{
							text.X = nextX;
						}

						textList.Add(text);
					}
				}

				// The X coordinate of all lines in the first group are always the same (equal to the element's
				// location).
				if (i > 0)
				{
					textList.ForEach(t => t.X = nextX);
				}

				// This sets up the X position for the next group.
				if (i < groupOrder.Count - 1)
				{
					nextX += rawList.Max(s => font.Measure(s).x) + blockSpacing;
				}
			}
		}
	}
}
