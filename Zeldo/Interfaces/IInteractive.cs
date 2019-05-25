using Zeldo.Entities.Core;

namespace Zeldo.Interfaces
{
	public interface IInteractive
	{
		bool InteractionEnabled { get; }

		void OnInteract(Entity entity);
	}
}
