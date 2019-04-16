using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using Engine.Core._2D;
using Engine.Graphics;
using Engine.Interfaces._2D;
using GlmSharp;

namespace Zeldo.UI.Screens
{
	public class ItemIcon : IClickable, IRenderable2D
	{
		private static Texture spritesheet;

		static ItemIcon()
		{
			spritesheet = ContentCache.GetTexture("Items.png");
		}

		private Sprite sprite;
		private SpriteText label;

		public ItemIcon(string label, Bounds2D sourceRect)
		{
			this.label = new SpriteText("Default", label, Alignments.Center);

			sprite = new Sprite(spritesheet, sourceRect);
			Bounds = new Bounds2D(64);
		}

		public ivec2 Location
		{
			get => (ivec2)sprite.Position;
			set
			{
				sprite.Position = value;
				label.Position = value + new ivec2(0, 48);
			}
		}

		public Bounds2D Bounds { get; }

		public void OnHover(ivec2 mouseLocation)
		{
		}

		public void OnUnhover()
		{
		}

		public void OnClick(ivec2 mouseLocation)
		{
		}

		public bool Contains(ivec2 mouseLocation)
		{
			return Bounds.Contains(mouseLocation);
		}

		public void Draw(SpriteBatch sb)
		{
			sprite.Draw(sb);
			label.Draw(sb);
		}
	}
}
