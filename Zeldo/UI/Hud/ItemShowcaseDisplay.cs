using Engine;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.UI;
using GlmSharp;
using Zeldo.Items;

namespace Zeldo.UI.Hud
{
	public class ItemShowcaseDisplay : CanvasElement
	{
		private Sprite itemSprite;
		private SpriteText itemText;

		private int textOffset;

		private float fadeTime;
		private float lifetime;

		public ItemShowcaseDisplay()
		{
			const string Prefix = "item.showcase.";

			itemText = new SpriteText("Debug");
			fadeTime = Properties.GetFloat(Prefix + "fade.time");
			lifetime = Properties.GetFloat(Prefix + "lifetime");
			textOffset = Properties.GetInt(Prefix + "text.offset");
		}

		public override ivec2 Location
		{
			get => base.Location;
			set
			{
				itemSprite.Position = value;
				itemText.Position = value + new vec2(0, textOffset);

				base.Location = value;
			}
		}

		public void Refresh(ItemData item)
		{
		}

		public override void Draw(SpriteBatch sb)
		{
			itemSprite.Draw(sb);
			itemText.Draw(sb);
		}
	}
}
