using System;
using Engine.Graphics._2D;

namespace Engine.Interfaces._2D
{
	public interface IRenderTargetUser2D : IDisposable
	{
		void DrawTargets(SpriteBatch sb);
	}
}
