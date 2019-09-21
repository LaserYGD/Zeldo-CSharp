using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeldo.Settings
{
	public class ControlSettings
	{
		public delegate void ApplyHandler(ControlSettings settings);

		public int MouseSensitivity { get; set; }

		public bool InvertX { get; set; }
		public bool InvertY { get; set; }

		// For accessibility (and preference), some skills (like grab and ascend) can be used as either a toggle or a
		// hold.
		public bool UseToggleAscend { get; set; }
		public bool UseToggleBlock { get; set; }
		public bool UseToggleGrab { get; set; }

		public event ApplyHandler ApplyEvent;

		public void Apply()
		{
			ApplyEvent?.Invoke(this);
		}
	}
}
