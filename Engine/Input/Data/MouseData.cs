﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;

namespace Engine.Input.Data
{
	public class MouseData : InputData
	{
		private InputStates[] buttons;

		public MouseData(ivec2 location, ivec2 previousLocation, InputStates[] buttons)
		{
			this.buttons = buttons;

			Location = location;
			PreviousLocation = previousLocation;
		}

		public ivec2 Location { get; }
		public ivec2 PreviousLocation { get; }

		public override bool AnyPressed()
		{
			return false;
		}

		public override bool Query(int data, InputStates state)
		{
			return (buttons[data] & state) == state;
		}
	}
}
