using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Input.Data;
using Engine.Interfaces;
using GlmSharp;

namespace Engine.Input
{
	public class InputProcessor : IDynamic
	{
		private InputStates[] buttons;

		private ivec2 mouseLocation;
		private ivec2 previousMouseLocation;

		public InputProcessor()
		{
			//buttons = Enumerable.Repeat(InputStates.Released, GLFW_MOUSE_BUTTON_LAST).ToArray();
		}

		public void OnMouseButtonPress(int button)
		{
		}

		public void OnMouseButtonRelease(int button)
		{
		}

		public void OnMouseMove(int x, int y)
		{
			mouseLocation.x = x;
			mouseLocation.y = y;
		}

		public void Update(float dt)
		{
		}

		private MouseData GetMouseData()
		{
			MouseData data = new MouseData(mouseLocation, previousMouseLocation, buttons);

			previousMouseLocation = mouseLocation;

			return data;
		}
	}
}
