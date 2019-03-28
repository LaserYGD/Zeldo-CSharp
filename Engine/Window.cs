using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSGL.Glfw3;

namespace Engine
{
	public class Window
	{
		public Window(string title, int width, int height)
		{
			Width = width;
			Height = height;
			Address = glfwCreateWindow(width, height, title, IntPtr.Zero, IntPtr.Zero);
		}

		public int Width { get; }
		public int Height { get; }

		public IntPtr Address { get; }
	}
}
