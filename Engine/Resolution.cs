using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Messaging;
using GlmSharp;

namespace Engine
{
	public static class Resolution
	{
		private static ivec2 windowDimensions;

		public static ivec2 Dimensions => new ivec2(800, 600);
		public static ivec2 WindowDimensions
		{
			get => windowDimensions;
			set
			{
				windowDimensions = value;

				MessageSystem.Send(CoreMessageTypes.Resize, value);
			}
		}
	}
}
