using Zeldo.Entities.Core;

namespace Zeldo.Interfaces
{
	public interface IInteractive
	{
		bool IsInteractionEnabled { get; }

		void OnInteract(Entity entity);
	}
}
