using System;

namespace Engine.Interfaces
{
	public interface IRenderTargetUser : IDisposable
	{
		void DrawTargets();
	}
}
