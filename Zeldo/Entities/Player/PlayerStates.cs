using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeldo.Entities.Player
{
	// These states are meant to be mutually exclusive.
	public enum PlayerStates
	{
		// This means airbore *without* being in another listed state.
		Airborne,
		Ascending,
		Idle,
		Jumping,
		OnWall,
		Running,
		Sliding,
		Grabbing
	}
}
