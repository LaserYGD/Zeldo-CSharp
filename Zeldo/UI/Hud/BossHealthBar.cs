using Engine;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.UI;
using GlmSharp;

namespace Zeldo.UI.Hud
{
	public class BossHealthBar : CanvasElement
	{
		public BossHealthBar()
		{
			IsCentered = true;
			nameText = new SpriteText("Debug");
		}

		private SpriteText nameText;

		public int Health { get; set; }
		public int MaxHealth { get; set; }

		public string Name
		{
			set => nameText.Value = value;
		}

		public override ivec2 Location
		{
			get => base.Location;
			set
			{
				int spacing = Properties.GetInt("boss.name.spacing");
				int x = Properties.GetInt("boss.name.x");

				nameText.Position = Location - new ivec2(-x, nameText.Size + spacing);

				base.Location = value;
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			nameText.Draw(sb);
			sb.Draw(new Bounds2D(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height), Color.Yellow);
		}
	}
}
