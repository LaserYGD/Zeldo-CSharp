using System;

namespace Zeldo.Entities
{
	[Flags]
	public enum PlayerStates
	{
		Idle = 1<<0,
		Jumping = 1<<1,
		Running = 1<<2,
		Sliding = 1<<3
	}
}
