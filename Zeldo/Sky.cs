using Engine;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Interfaces._2D;

namespace Zeldo
{
	public class Sky : IRenderTargetUser2D
	{
		public Sky()
		{
			Target = new RenderTarget(Resolution.RenderDimensions, RenderTargetFlags.Color);
		}

		public RenderTarget Target { get; }

		public void Dispose()
		{
			Target.Dispose();
		}

		public void DrawTargets(SpriteBatch sb)
		{
			Target.Apply();
		}
	}
}
