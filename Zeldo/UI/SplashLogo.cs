using Engine;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Timing;
using Engine.UI;

namespace Zeldo.UI
{
	public class SplashLogo : CanvasElement
	{
		private SpriteText spriteText;
		private SingleTimer timer;

		private float fadeTime;
		private float lifetime;

		// The logo has three states: fading in (0), holding (1), and fading out (2).
		private int state;

		public SplashLogo()
		{
			Centered = true;
			spriteText = new SpriteText("Debug", "[Grimgaming]", Alignments.Center);
			spriteText.UseLiteralMeasuring = true;

			fadeTime = Properties.GetFloat("splash.fade.time");
			lifetime = Properties.GetFloat("splash.lifetime");

			timer = new SingleTimer(t =>
			{
				state++;

				if (state < 3)
				{
					timer.Duration = state == 1 ? lifetime : fadeTime;
				}
				else
				{
					timer.IsComplete = state == 2;
				}
			}, fadeTime);

			timer.Repeatable = true;
			timer.Tick = t =>
			{
				// Holding between fades.
				if (state == 1)
				{
					return;
				}

				spriteText.Color = Color.Lerp(Color.Transparent, Color.White, state == 0 ? t : 1 - t);
			};

			Components.Add(timer);
		}

		public override void Draw(SpriteBatch sb)
		{
			spriteText.Draw(sb);
		}
	}
}
