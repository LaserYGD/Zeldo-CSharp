using System;

namespace Engine.Sensors
{
	[Flags]
	public enum SensorGroups
	{
		None = 0,
		Control = 1<<0,
		Hitbox = 1<<1,
		Interaction = 1<<2
	}
}
