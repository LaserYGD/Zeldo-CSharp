using Engine;
using Engine.Core._2D;
using Engine.Interfaces;

namespace Zeldo
{
	public class Sky : IRenderTargetUser
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

		public void DrawTargets()
		{
			Target.Apply();
		}
	}
}
