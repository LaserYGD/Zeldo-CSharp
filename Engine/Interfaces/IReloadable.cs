using System;
using Engine.Props;

namespace Engine.Interfaces
{
	public interface IReloadable : IDisposable
	{
		void Reload(PropertyAccessor accessor);
	}
}
