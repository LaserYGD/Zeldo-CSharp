using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core._2D;
using Engine.Graphics;
using Engine.Graphics._2D;
using Engine.UI;
using GlmSharp;

namespace Zeldo.UI.Screens
{
	public class InventoryScreen : CanvasElement
	{
		private const int Width = 600;
		private const int Height = 400;

		private ItemIcon bowIcon;
		private Bounds2D bounds;

		public InventoryScreen()
		{
			bowIcon = new ItemIcon("Bow", null);
			bounds = new Bounds2D(Width, Height);
		}

		public override ivec2 Location
		{
			get => throw new NotImplementedException();
			set
			{
				bounds.Center = value;
				bowIcon.Location = bounds.Location + new ivec2(40);
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			bowIcon.Draw(sb);
		}
	}
}
