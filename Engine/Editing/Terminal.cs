using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Input.Data;
using Engine.Input.Processors;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Props;
using Engine.UI;
using GlmSharp;
using static Engine.GLFW;

namespace Engine.Editing
{
	public class Terminal : CanvasElement, IReceiver
	{
		private const int BackIndex = 0;
		private const int TextIndex = 1;
		private const int SuccessIndex = 2;
		private const int FailureIndex = 3;

		public delegate bool CommandProcessor(string[] args, out string result);

		private Dictionary<string, CommandProcessor> commands;
		private TextProcessor textProcessor;
		private Color[] colors;
		private SpriteFont font;
		private SpriteText currentLine;
		private List<SpriteText> lines;
		private List<string> oldCommands;

		// The terminal uses a monospace font.
		private int charWidth;
		private int padding;
		private int storedIndex;

		public Terminal()
		{
			colors = new Color[4];
			commands = new Dictionary<string, CommandProcessor>();
			font = ContentCache.GetFont("Terminal");
			currentLine = new SpriteText(font);
			lines = new List<SpriteText>();
			oldCommands = new List<string>();
			charWidth = font.Measure("A").x;
			storedIndex = -1;

			var accessor = Properties.Access(this);

			padding = accessor.GetInt("terminal.padding", this, false);
			textProcessor = new TextProcessor();
			textProcessor.Submit = Submit;

			AddDefaultCommands();

			MessageSystem.Subscribe(this, CoreMessageTypes.ResizeWindow, (messageType, data, dt) =>
			{
				// The terminal is always resized to fit the window width.
				Width = ((ivec2)data).x;
			});

			MessageSystem.Subscribe(this, CoreMessageTypes.Keyboard, (messageType, data, dt) =>
			{
				ProcessKeyboard((KeyboardData)data);
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }

		// TODO: Add other default commands (like "help" and "commands").
		private void AddDefaultCommands()
		{
			// Exit command (alias 'quit').
			Add((string[] args, out string result) =>
			{
				result = null;

				MessageSystem.Send(CoreMessageTypes.Exit);

				return true;
			}, "exit", "quit");

			// Property commands.
			Add("echo", Properties.TryEcho);
			Add("set", Properties.TryModify);
		}

		public override void Initialize()
		{
			currentLine.Position = new vec2(padding, Height - padding - font.Size);
		}

		public override void Reload(PropertyAccessor accessor)
		{
			// The array is initialized in the constructor (before accessing properties).
			colors[0] = accessor.GetColor("terminal.back.color", this);
			colors[1] = accessor.GetColor("terminal.text.color", this);
			colors[2] = accessor.GetColor("terminal.success.color", this);
			colors[3] = accessor.GetColor("terminal.failure.color", this);
		}

		public void Add(string command, CommandProcessor processor)
		{
			Debug.Assert(command != null, "Command can't be null.");
			Debug.Assert(processor != null, "Command processor function can't be null.");
			Debug.Assert(!commands.ContainsKey(command), $"Duplicate command '{command}'.");

			Add($"Command '{command}' added.", colors[TextIndex]);
			commands.Add(command, processor);
		}

		// This version is useful when multiple commands should alias to the same processor.
		public void Add(CommandProcessor processor, params string[] commands)
		{
			foreach (var command in commands)
			{
				Add(command, processor);
			}
		}

		public void Remove(string command)
		{
			if (commands.ContainsKey(command))
			{
				Add($"Command '{command}' removed.", colors[TextIndex]);
				commands.Remove(command);
			}
		}

		private void ProcessKeyboard(KeyboardData data)
		{
			if (data.Query(GLFW_KEY_GRAVE_ACCENT, InputStates.PressedThisFrame))
			{
				IsVisible = !IsVisible;

				// TODO: Swap input control when the terminal is toggled.
				// When the terminal is active, it takes over input control.
				return;
			}

			if (!IsVisible)
			{
				return;
			}

			bool up = data.Query(GLFW_KEY_UP, InputStates.PressedThisFrame);
			bool down = data.Query(GLFW_KEY_DOWN, InputStates.PressedThisFrame);

			if (up ^ down)
			{
				string text = null;

				if (up && storedIndex < oldCommands.Count - 1)
				{
					storedIndex++;
					text = oldCommands[oldCommands.Count - storedIndex - 1];
				}
				else if (down && storedIndex > -1)
				{
					storedIndex--;
					text = storedIndex >= 0 ? oldCommands[oldCommands.Count - storedIndex - 1] : "";
				}

				// If an old command is selected, other keys are ignored on the current frame.
				if (text != null)
				{
					currentLine.Value = text;
					textProcessor.Value = text;

					return;
				}
			}

			textProcessor.ProcessKeyboard(data);
			currentLine.Value = textProcessor.Value;
		}

		private bool Submit(string value)
		{
			var success = Run(value, out var result);

			// A null result string can be returned in rare cases (such as when exiting the program).
			if (success && result == null)
			{
				return true;
			}

			var color = success ? colors[SuccessIndex] : colors[FailureIndex];

			Add(result, color);

			return true;
		}

		private void Add(string s, Color color)
		{
			var line = new SpriteText(font, s);

			line.Color = color;
			lines.Add(line);

			var start = new vec2(padding, Height - font.Size * 2 - padding * 3 - 4);
			var spacing = new vec2(0, font.Size + padding);

			for (int i = lines.Count - 1; i >= 0; i--)
			{
				lines[i].Position = start - spacing * (lines.Count - i - 1);
			}
		}

		private bool Run(string input, out string result)
		{
			var tokens = input.Split(' ');
			var command = tokens[0];

			oldCommands.Add(input);
			storedIndex = -1;

			if (!commands.TryGetValue(command, out var processor))
			{
				result = $"Unrecognized command '{command}'.";

				return false;
			}

			var args = new string[tokens.Length - 1];

			Array.Copy(tokens, 1, args, 0, args.Length);

			return processor(args, out result);
		}

		public override void Dispose()
		{
			MessageSystem.Unsubscribe(this);
		}

		public override void Draw(SpriteBatch sb)
		{
			var p = currentLine.Position + new vec2(charWidth * textProcessor.Cursor + 1, 0);
			var l1 = new vec2(0, Height - font.Size - padding * 2 - 1);
			var l2 = l1 + new vec2(Width, 0);
			var textColor = colors[TextIndex];

			sb.Fill(bounds, colors[BackIndex]);
			sb.DrawLine(p, p + new vec2(0, font.Size), textColor);
			sb.DrawLine(l1, l2, textColor);

			lines.ForEach(l => l.Draw(sb));
			currentLine.Draw(sb);
		}
	}
}
