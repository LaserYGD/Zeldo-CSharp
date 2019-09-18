using Engine;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.UI;
using Zeldo.Data;

namespace Zeldo.UI.Screens.Title
{
	public class SaveSlotUI : CanvasElement
	{
		private SaveSlot saveSlot;
		private SpriteText percentage;

		public SaveSlotUI()
		{
			Anchor = Alignments.None;
			saveSlot = new SaveSlot(null);

			SpriteFont font = ContentCache.GetFont("Debug");
			percentage = new SpriteText(font);

			if (saveSlot.ShowPercentage)
			{
				percentage.Value = $"{saveSlot.Progression.ComputePercentage():F2}%";
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			if (saveSlot.ShowPercentage)
			{
				percentage.Draw(sb);
			}
		}
	}
}
