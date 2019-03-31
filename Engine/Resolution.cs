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
		private static ivec2 dimensions;

		public static ivec2 Dimensions
		{
			get => dimensions;
			set
			{
				dimensions = value;

				MessageSystem.Send(CoreMessageTypes.Resize, value);
			}
		}
		public static ivec2 WindowDimensions { get; set; }
	}
}
