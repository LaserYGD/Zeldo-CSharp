using Engine.Interfaces._3D;
using Zeldo.Entities.Core;

namespace Zeldo.Interfaces
{
	public interface IInteractive : IPositionable3D
	{
		bool IsInteractionEnabled { get; }
		bool RequiresFacing { get; }

		void OnInteract(Entity entity);
	}
}
