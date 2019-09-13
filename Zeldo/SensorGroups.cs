using System;

namespace Zeldo
{
	[Flags]
	public enum SensorGroups
	{
		None = 0,
		DamageSource = 1<<2,
		Hitbox = 1<<4,
		Interaction = 1<<5,
		Player = 1<<6,
		Target = 1<<7
	}
}
