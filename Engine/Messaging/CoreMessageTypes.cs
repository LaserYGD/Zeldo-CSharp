using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Messaging
{
	public static class CoreMessageTypes
	{
		// These values are arbitrary, but large enough that custom message types can take up smaller ranges.
		public const int Exit = 1000;
		public const int Input = 1001;
		public const int Keyboard = 1002;
		public const int Mouse = 1003;
		public const int ResizeRender = 1004;
		public const int ResizeWindow = 1005;
	}
}
