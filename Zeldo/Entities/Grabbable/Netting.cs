using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Grabbable
{
	public class Netting : Entity, IGrabbable
	{
		public Netting() : base(EntityGroups.Object)
		{
		}

		public GrabTypes GrabType => GrabTypes.Netting;
	}
}
