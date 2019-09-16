using Zeldo.UI;

namespace Zeldo.Loops
{
	public class SplashLoop : GameLoop
	{
		public SplashLoop() : base(LoopTypes.Splash)
		{
		}

		public override void Initialize()
		{
			// The canvas is intentionally not cleared here under the assumption that if the splash loop is created,
			// it'll be the first one used (meaning that the canvas will already be clear). 
			canvas.Add(new SplashLogo());
		}

		public override void Update(float dt)
		{
			canvas.Update(dt);
		}

		public override void Draw()
		{
			canvas.Draw(sb);
		}
	}
}
