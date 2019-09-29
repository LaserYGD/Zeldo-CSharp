using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
	// This enum originated as "ladder zones", used to allow the player to grab ladders from any direction (but always
	// whip around to the front). Turns out that zone concept is applicable to other scenarios as well.
	public enum Proximities
	{
		Front,
		Back,
		Side
	}
}
