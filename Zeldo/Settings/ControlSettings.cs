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

		// For accessibility (and preference), some skills (like grab and ascend) can be used as either a toggle or a
		// hold.
		public bool UseToggleGrab { get; set; }
		public bool UseToggleAscend { get; set; }
	}
}
