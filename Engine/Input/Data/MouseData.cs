using System.Diagnostics;
using System.Linq;
using GlmSharp;
using static Engine.GLFW;

namespace Engine.Input.Data
{
	public class MouseData : InputData
	{
		private InputStates[] buttons;

		public MouseData(ivec2 location, ivec2 previousLocation, InputStates[] buttons) : base(InputTypes.Mouse)
		{
			this.buttons = buttons;

			Location = location;
			PreviousLocation = previousLocation;
		}

		public ivec2 Location { get; }
		public ivec2 PreviousLocation { get; }

		public override InputStates this[int data]
		{
			get
			{
				Debug.Assert(data >= 0 && data <= GLFW_MOUSE_BUTTON_LAST, "Invalid mouse button (outside maximum range).");

				return buttons[data];
			}
		}

		public override bool AnyPressed()
		{
			// Mouse movement, holding down buttons, or using the scroll wheel don't count as an "any press" (for the
			// purposes of something like a "Press Start" screen).
			return buttons.Any(b => b == InputStates.PressedThisFrame);
		}

		public override bool Query(int data, InputStates state)
		{
			Debug.Assert(data >= 0 && data <= GLFW_MOUSE_BUTTON_LAST, "Invalid mouse button (outside maximum range).");

			return (buttons[data] & state) == state;
		}
	}
}
