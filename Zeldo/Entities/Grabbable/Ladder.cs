using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Grabbable
{
	public class Ladder : Entity, IGrabbable
	{
		public Ladder() : base(EntityGroups.Object)
		{
		}

		public GrabTypes GrabType => GrabTypes.Ladder;

		public float Top { get; }
		public float Bottom { get; }
	}
}
