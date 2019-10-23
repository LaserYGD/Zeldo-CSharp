using System;

namespace Zeldo.Entities.Player
{
	[Flags]
	public enum PlayerStates
	{
		Airborne = 1<<0,
		Ascending = 1<<1,
		Attacking = 1<<2,
		Blocking = 1<<3,
		Grabbing = 1<<4,
		Idle = 1<<5,
		Interacting = 1<<6,
		Jumping = 1<<7,
		OnGround = 1<<8,
		OnLadder = 1<<9,
		OnPlatform = 1<<10,
		OnWall = 1<<11,
		Running = 1<<12,
		Sliding = 1<<13,
		Vaulting = 1<<14
	}
}
