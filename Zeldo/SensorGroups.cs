using System;

namespace Zeldo
{
	// TODO: Are these needed?
	[Flags]
	public enum SensorGroups
	{
		None = 0,
		DamageSource = 1<<1,
		DamageBlock = 1<<2,
		Hitbox = 1<<3,
		Interaction = 1<<4,
		Player = 1<<5,
		Target = 1<<6
	}
}
