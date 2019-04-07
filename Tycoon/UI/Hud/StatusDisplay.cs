using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core._2D;
using Engine.Graphics;
using Engine.UI;

namespace Tycoon.UI.Hud
{
	public class StatusDisplay : CanvasElement
	{
		private SpriteText guests;

		public StatusDisplay()
		{
			guests = new SpriteText("Default");
		}

		public override void Draw(SpriteBatch sb)
		{
			guests.Draw(sb);
		}
	}
}
