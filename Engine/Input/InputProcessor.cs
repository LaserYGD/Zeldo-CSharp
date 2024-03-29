﻿using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using GlmSharp;
using static Engine.GLFW;

namespace Engine.Input
{
	public class InputProcessor : IDynamic
	{
		private InputStates[] buttons;
		private InputStates[] keys;

		private List<KeyPress> keyPresses;
		private ivec2 mouseLocation;
		private ivec2 previousMouseLocation;

		// On the first frame of mouse movement, the mouse's previous location is artificially set to the current
		// location in order to avoid a large, false delta.
		private bool firstMouseMovement;

		public InputProcessor()
		{
			buttons = Enumerable.Repeat(InputStates.Released, GLFW_MOUSE_BUTTON_LAST).ToArray();
			keys = Enumerable.Repeat(InputStates.Released, GLFW_KEY_LAST).ToArray();
			keyPresses = new List<KeyPress>();
			firstMouseMovement = true;
		}

		internal void KeyCallback(IntPtr window, int key, int scancode, int action, int mods)
		{
			if (key == -1)
			{
				return;
			}

			if (action == GLFW_PRESS)
			{
				keys[key] = InputStates.PressedThisFrame;
				keyPresses.Add(new KeyPress(key, (KeyModifiers)mods));
			}
		}

		internal void OnKeyPress(int key, int mods)
		{
			keys[key] = InputStates.PressedThisFrame;
			keyPresses.Add(new KeyPress(key, (KeyModifiers)mods));
		}

		internal void OnKeyRelease(int key)
		{
			keys[key] = InputStates.ReleasedThisFrame;
		}

		internal void OnMouseButtonPress(int button)
		{
			buttons[button] = InputStates.PressedThisFrame;
		}

		internal void OnMouseButtonRelease(int button)
		{
			buttons[button] = InputStates.ReleasedThisFrame;
		}

		internal void OnMouseMove(int x, int y)
		{
			mouseLocation.x = x;
			mouseLocation.y = y;
		}

		public void Update(float dt)
		{
			var mouseData = GetMouseData();
			var keyboardData = GetKeyboardData();

			FullInputData fullData = new FullInputData();
			fullData.Add(InputTypes.Mouse, mouseData);
			fullData.Add(InputTypes.Keyboard, keyboardData);

			MessageSystem.Send(CoreMessageTypes.Keyboard, keyboardData, dt);
			MessageSystem.Send(CoreMessageTypes.Mouse, mouseData, dt);
			MessageSystem.Send(CoreMessageTypes.Input, fullData, dt);
		}

		private KeyboardData GetKeyboardData()
		{
			KeyboardData data = new KeyboardData((InputStates[])keys.Clone(), keyPresses.ToArray());

			for (int i = 0; i < keys.Length; i++)
			{
				switch (keys[i])
				{
					case InputStates.PressedThisFrame: keys[i] = InputStates.Held; break;
					case InputStates.ReleasedThisFrame: keys[i] = InputStates.Released; break;
				}
			}

			keyPresses.Clear();

			return data;
		}

		private MouseData GetMouseData()
		{
			if (firstMouseMovement && mouseLocation != ivec2.Zero)
			{
				previousMouseLocation = mouseLocation;
				firstMouseMovement = false;
			}

			MouseData data = new MouseData(mouseLocation, previousMouseLocation, (InputStates[])buttons.Clone());
			previousMouseLocation = mouseLocation;

			for (int i = 0; i < buttons.Length; i++)
			{
				switch (buttons[i])
				{
					case InputStates.PressedThisFrame: buttons[i] = InputStates.Held; break;
					case InputStates.ReleasedThisFrame: buttons[i] = InputStates.Released; break;
				}
			}

			return data;
		}
	}
}
