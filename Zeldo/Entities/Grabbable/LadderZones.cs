using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeldo.Entities.Grabbable
{
	// Ladder zones are used to help the player whip around to the correct side of the ladder, even if grabbed from
	// the side or back.
	public enum LadderZones
	{
		Front,
		Back,
		Side
	}
}
