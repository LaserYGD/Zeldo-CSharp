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
			IsCentered = true;
			spriteText = new SpriteText("Debug", "[Grimgaming]", Alignments.Center);
			spriteText.UseLiteralMeasuring = true;

			// TODO: Pull properties.
			//fadeTime = Properties.GetFloat("splash.fade.time");
			//lifetime = Properties.GetFloat("splash.lifetime");

			timer = new SingleTimer(t =>
			{
				state++;

				if (state < 3)
				{
					timer.Duration = state == 1 ? lifetime : fadeTime;
				}
				else
				{
					// TODO: Transition game states once the logo has fully faded out.
				}
			}, fadeTime, TimerFlags.None);

			timer.IsRepeatable = true;
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
