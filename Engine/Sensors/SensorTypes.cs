﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Sensors
{
	[Flags]
	public enum SensorTypes
	{
		All = 3,
		Entity = 1,
		None = 0,
		Zone = 2
	}
}
