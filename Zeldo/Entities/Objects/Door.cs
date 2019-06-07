using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Objects
{
	public class Door : Entity, IInteractive
	{
		private bool isLocked;

		public Door() : base(EntityGroups.Object)
		{
		}

		public bool IsInteractionEnabled => true;

		public void OnInteract(Entity entity)
		{
			// Open and transition the camera if appropriate
		}
	}
}
