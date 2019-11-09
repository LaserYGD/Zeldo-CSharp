using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.UI;
using Zeldo.Entities.Player;

namespace Zeldo.UI.Hud
{
	public class PlayerHealthDisplay : CanvasElement
	{
		private SpriteText spriteText;

		public PlayerHealthDisplay()
		{
			spriteText = new SpriteText("Debug");
			Attach(spriteText);
		}

		public PlayerCharacter Player { get; set; }

		public override void Update(float dt)
		{
			spriteText.Value = $"Health: {Player.Health} / {Player.MaxHealth}";
		}

		public override void Draw(SpriteBatch sb)
		{
			spriteText.Draw(sb);
		}
	}
}
