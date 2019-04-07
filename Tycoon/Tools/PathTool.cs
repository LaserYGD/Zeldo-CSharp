using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tycoon.Tools
{
	public class PathTool : Tool
	{
		public PathTool()
		{
			Mode = PathModes.Singular;
		}

		public PathModes Mode { get; }

		public override void UsePrimary()
		{
			// Ray trace on the cursor to determine which surface you're looking at
			// While working on that surface, all visible blocking geometry is made almost fully transparent
			// You can zoom the camera in and out while working (using the scroll wheel)

		}

		public void UseSecondary()
		{
		}
	}
}
