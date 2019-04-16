using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Input.Data;
using static Engine.GLFW;

namespace Zeldo.UI.Screens
{
	public class ScreenControls
	{
		public ScreenControls()
		{
			Inventory = new List<InputBind> { new InputBind(InputTypes.Keyboard, GLFW_KEY_I) };
			Map = new List<InputBind> { new InputBind(InputTypes.Keyboard, GLFW_KEY_M) };
		}

		public List<InputBind> Inventory { get; }
		public List<InputBind> Map { get; }
	}
}
