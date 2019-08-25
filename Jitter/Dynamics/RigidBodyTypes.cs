using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jitter.Dynamics
{
	// CUSTOM: Added as a way to support kinematic bodies.
	public enum RigidBodyTypes
	{
		Dynamic = 0,
		Kinematic = 1,
		Static = 2
	}
}
