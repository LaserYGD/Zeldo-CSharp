using System.Collections.Generic;
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
			Attack = new List<InputBind> { new InputBind(InputTypes.Mouse, GLFW_MOUSE_BUTTON_LEFT) };
			Jump = new List<InputBind> { new InputBind(InputTypes.Keyboard, GLFW_KEY_SPACE) };
			Grab = new List<InputBind> { new InputBind(InputTypes.Keyboard, GLFW_KEY_LEFT_SHIFT) };
			Interact = new List<InputBind> { new InputBind(InputTypes.Keyboard, GLFW_KEY_E) };
		}

		public List<InputBind> RunLeft { get; }
		public List<InputBind> RunRight { get; }
		public List<InputBind> RunUp { get; }
		public List<InputBind> RunDown { get; }
		public List<InputBind> Attack { get; }
		public List<InputBind> Jump { get; }
		public List<InputBind> Grab { get; }
		public List<InputBind> Interact { get; }
	}
}
