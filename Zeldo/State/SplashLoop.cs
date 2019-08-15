using Zeldo.UI;

namespace Zeldo.State
{
	public class SplashLoop : GameLoop
	{
		public override void Initialize()
		{
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
