using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeldo.Settings
{
	public class ControlSettings
	{
		public int MouseSensitivity { get; set; }

		public bool InvertX { get; set; }
		public bool InvertY { get; set; }

		// For accessibility, grabbing can be changed to a toggle (rather than held).
		public bool UseToggleGrab { get; set; }
	}
}
