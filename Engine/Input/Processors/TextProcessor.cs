using System;
using System.Text;
using Engine.Input.Data;
using static Engine.GLFW;

namespace Engine.Input.Processors
{
	public class TextProcessor
	{
		private static readonly char[] NumericSpecials = { ')', '!', '@', '#', '$', '%', '^', '&', '*', '(' };

		private StringBuilder builder;

		// Cursor position is before the character (i.e. position zero is the start of the line).
		private int cursor;
		private bool insertMode;

		public TextProcessor()
		{
			builder = new StringBuilder();
		}

		public string Value
		{
			get => builder.ToString();
			set
			{
				builder.Clear();
				builder.Append(value);

				// When value is set directly, the cursor is moved to the end of the line.
				cursor = value.Length;
			}
		}

		public int Cursor => cursor;
		public bool InsertMode => insertMode;

		public Func<string, bool> Submit { get; set; }

		public void ProcessKeyboard(KeyboardData data)
		{
			// If enter is pressed, all other text processing is ignored on the current frame.
			if (Value.Length > 0 && Submit != null && data.Query(InputStates.PressedThisFrame, GLFW_KEY_ENTER,
				GLFW_KEY_KP_ENTER))
			{
				if (Submit(Value))
				{
					builder.Clear();
					cursor = 0;
				}

				return;
			}

			bool shift = data.Query(InputStates.Held, GLFW_KEY_LEFT_SHIFT, GLFW_KEY_RIGHT_SHIFT);
			bool capsLock = InputUtilities.IsEnabled(LockKeys.CapsLock);
			bool numLock = InputUtilities.IsEnabled(LockKeys.NumLock);

			// Even if numlock is off, special numpad functions can be activated by holding shift.
			if (!numLock)
			{
				numLock = shift;
			}

			bool home = data.Query(GLFW_KEY_HOME, InputStates.PressedThisFrame);
			bool end = data.Query(GLFW_KEY_END, InputStates.PressedThisFrame);
			bool left = data.Query(GLFW_KEY_LEFT, InputStates.PressedThisFrame);
			bool right = data.Query(GLFW_KEY_RIGHT, InputStates.PressedThisFrame);
			bool backspace = data.Query(GLFW_KEY_BACKSPACE, InputStates.PressedThisFrame);
			bool delete = data.Query(GLFW_KEY_DELETE, InputStates.PressedThisFrame);

			if (!numLock)
			{
				home |= data.Query(GLFW_KEY_KP_7, InputStates.PressedThisFrame);
				end |= data.Query(GLFW_KEY_KP_1, InputStates.PressedThisFrame);
				left |= data.Query(GLFW_KEY_KP_4, InputStates.PressedThisFrame);
				right |= data.Query(GLFW_KEY_KP_6, InputStates.PressedThisFrame);
				delete |= data.Query(GLFW_KEY_KP_DECIMAL, InputStates.PressedThisFrame);
			}

			// If home and end are pressed on the same frame, home takes priority.
			if (home)
			{
				cursor = 0;
			}
			else if (end)
			{
				cursor = builder.Length;
			}

			if (left ^ right)
			{
				if (left)
				{
					cursor = cursor > 0 ? --cursor : 0;
				}
				else
				{
					cursor = cursor < builder.Length ? ++cursor : builder.Length;
				}
			}

			// Backspace and delete are handled after moving the cursor and before adding new characters.
			if (backspace && builder.Length > 0 && cursor > 0)
			{
				builder.Remove(cursor - 1, 1);
				cursor--;

				return;
			}

			if (delete && builder.Length > 0 && cursor < builder.Length)
			{
				builder.Remove(cursor, 1);
			}

			// Changes to insert mode take effect immediately (e.g. pressing Insert and A will 
			if (data.Query(GLFW_KEY_INSERT, InputStates.PressedThisFrame))
			{
				insertMode = !insertMode;
			}

			foreach (var keyPress in data.KeysPressedThisFrame)
			{
				char? character = GetCharacter(keyPress.Key, shift, capsLock, numLock);

				if (character.HasValue)
				{
					if (insertMode && cursor < builder.Length)
					{
						builder[cursor] = character.Value;
					}
					else
					{
						builder.Insert(cursor, character.Value);
					}

					cursor++;
				}
			}
		}

		private char? GetCharacter(int key, bool shift, bool capsLock, bool numLock)
		{
			char c = (char)key;

			if (key >= GLFW_KEY_A && key <= GLFW_KEY_Z)
			{
				if (capsLock)
				{
					shift = !shift;
				}

				return shift ? c : char.ToLower(c);
			}

			if (key >= GLFW_KEY_0 && key <= GLFW_KEY_9)
			{
				return !shift ? c : NumericSpecials[c - '0'];
			}

			// Special numpad functions (like moving the cursor) are handled before reaching this point.
			if (numLock)
			{
				if (key >= GLFW_KEY_KP_0 && key <= GLFW_KEY_KP_9)
				{
					return (char)(c - (GLFW_KEY_KP_0 - GLFW_KEY_0));
				}

				if (key == GLFW_KEY_KP_DECIMAL)
				{
					return '.';
				}
			}

			switch (key)
			{
				case GLFW_KEY_COMMA: return shift ? '<' : ',';
				case GLFW_KEY_PERIOD: return shift ? '>' : '.';
				case GLFW_KEY_SLASH: return shift ? '?' : '/';
				case GLFW_KEY_SEMICOLON: return shift ? ':' : ';';
				case GLFW_KEY_APOSTROPHE: return shift ? '"' : '\'';
				case GLFW_KEY_LEFT_BRACKET: return shift ? '{' : '[';
				case GLFW_KEY_RIGHT_BRACKET: return shift ? '}' : ']';
				case GLFW_KEY_BACKSLASH: return shift ? '|' : '\\';
				case GLFW_KEY_MINUS: return shift ? '_' : '-';
				case GLFW_KEY_EQUAL: return shift ? '+' : '=';
				case GLFW_KEY_GRAVE_ACCENT: return shift ? '~' : '`';

				// These keys work even if numlock is turned off.
				case GLFW_KEY_KP_ADD: return '+';
				case GLFW_KEY_KP_SUBTRACT: return '-';
				case GLFW_KEY_KP_MULTIPLY: return '*';
				case GLFW_KEY_KP_DIVIDE: return '/';

				case GLFW_KEY_SPACE: return ' ';
			}

			return null;
		}
	}
}
