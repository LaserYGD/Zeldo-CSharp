using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Input;
using static CSGL.CSGL;
using static CSGL.Glfw3;

namespace Engine
{
	public abstract class Game
	{
		private Window window;
		private InputProcessor inputProcessor;

		private float previousTime;
		private float accumulator;

		protected Game(string title)
		{
			csglLoadGlfw();
			csglLoadGL();

			glfwInit();
			glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 4);
			glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 5);

			window = new Window(title, 800, 600);
			inputProcessor = new InputProcessor();

			IntPtr windowAddress = window.Address;

			glfwMakeContextCurrent(windowAddress);
			glfwSetMouseButtonCallback(windowAddress, OnMouseButton);
		}

		private void OnMouseButton(IntPtr windowAddress, int button, int action, int mods)
		{
			switch (action)
			{
				case GLFW_PRESS: inputProcessor.OnMouseButtonPress(button); break;
				case GLFW_RELEASE: inputProcessor.OnMouseButtonRelease(button); break;
			}
		}

		public void Run()
		{
			const float Target = 1.0f / 60;

			float time = (float)glfwGetTime();

			accumulator += time - previousTime;
			previousTime = time;

			bool shouldUpdate = accumulator >= Target;

			if (!shouldUpdate)
			{
				return;
			}

			while (accumulator >= Target)
			{
				glfwPollEvents();
				Update(Target);
				accumulator -= Target;
			}

			Draw();

			glfwSwapBuffers(window.Address);
		}

		protected abstract void Update(float dt);
		protected abstract void Draw();
	}
}
