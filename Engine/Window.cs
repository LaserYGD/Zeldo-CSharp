using System;

namespace Engine
{
	public class Window
	{
		public Window(string title, int width, int height, IntPtr address)
		{
			Width = width;
			Height = height;
			Address = address;
		}

		public int Width { get; }
		public int Height { get; }

		public IntPtr Address { get; }
	}
}
