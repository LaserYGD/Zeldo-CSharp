using System;

namespace Zeldo.Entities.Core
{
	[Flags]
	public enum ActorFlags
	{
		UsesAir = 1<<0,
		UsesGround = 1<<1,
		UsesPlatforms = 1<<2,
		UsesSwimming = 1<<3
	}
}
