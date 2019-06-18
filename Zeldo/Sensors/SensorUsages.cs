using System;

namespace Zeldo.Sensors
{
	[Flags]
	public enum SensorUsages
	{
		None = 0,
		Control = 1<<0,
		Hitbox = 1<<1,
		Interaction = 1<<2
	}
}
