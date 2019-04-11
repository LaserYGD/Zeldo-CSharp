using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Input.Data;
using static Engine.GLFW;

namespace Zeldo.Entities
{
	public class PlayerControls
	{
		public PlayerControls()
		{
			RunLeft = new List<InputBind> { new InputBind(InputTypes.Keyboard, GLFW_KEY_A) };
			RunRight = new List<InputBind> { new InputBind(InputTypes.Keyboard, GLFW_KEY_D) };
			RunUp = new List<InputBind> { new InputBind(InputTypes.Keyboard, GLFW_KEY_W) };
			RunDown = new List<InputBind> { new InputBind(InputTypes.Keyboard, GLFW_KEY_S) };
		}

		public List<InputBind> RunLeft { get; }
		public List<InputBind> RunRight { get; }
		public List<InputBind> RunUp { get; }
		public List<InputBind> RunDown { get; }
	}
}
