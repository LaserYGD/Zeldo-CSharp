using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Mechanisms
{
	public class Lever : Entity, IInteractive
	{
		private bool isSwitchedOn;

		public Lever() : base(EntityGroups.Mechanism)
		{
		}

		public bool IsInteractionEnabled => true;
		public bool RequiresFacing => true;

		public void OnInteract(Entity entity)
		{
		}
	}
}
