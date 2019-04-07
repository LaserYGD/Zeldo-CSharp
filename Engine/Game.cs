using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Input;
using GlmSharp;
using static Engine.GLFW;

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
			glfwInit();
			glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 4);
			glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 4);
			glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

			IntPtr address = glfwCreateWindow(800, 600, title, IntPtr.Zero, IntPtr.Zero);

			if (address == IntPtr.Zero)
			{
				glfwTerminate();

				return;
			}

			window = new Window(title, 800, 600, address);
			inputProcessor = new InputProcessor();

			glfwMakeContextCurrent(address);
			//glfwSetMouseButtonCallback(address, OnMouseButton);
		}

		private void OnMouseButton(IntPtr windowAddress, int button, int action, int mods)
		{
			/*
			switch (action)
			{
				case GLFW_PRESS: inputProcessor.OnMouseButtonPress(button); break;
				case GLFW_RELEASE: inputProcessor.OnMouseButtonRelease(button); break;
			}
			*/
		}

		public void Run()
		{
			const float Target = 1.0f / 60;

			while (glfwWindowShouldClose(window.Address) == 0)
			{
				float time = (float)glfwGetTime();

				accumulator += time - previousTime;
				previousTime = time;

				bool shouldUpdate = accumulator >= Target;

				if (!shouldUpdate)
				{
					continue;
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

			glfwTerminate();
		}

		protected abstract void Update(float dt);
		protected abstract void Draw();
	}
}
