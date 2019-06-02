using System;
using Engine.View;

namespace Engine.Interfaces._3D
{
	public interface IRenderTargetUser3D : IDisposable
	{
		void DrawTargets();
	}
}
