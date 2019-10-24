using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jitter.Dynamics
{
	// CUSTOM: Added as a way to support kinematic bodies.
	public enum RigidBodyTypes
	{
        // Body types are compared numerically to determine which pairs should react to each other. As such, setting
        // values explicitly probably isn't needed, but it's safer anyway.
		Dynamic = 0,
		Kinematic = 1,

        // "Pseudo-static" is effectively only used for moving platforms. Pseudo-static bodies behave exactly like
        // static ones except that pseudo-static bodies also simulate velocity.
        PseudoStatic = 2,
		Static = 3
	}
}
