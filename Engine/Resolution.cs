using Engine.Messaging;
using GlmSharp;

namespace Engine
{
	public static class Resolution
	{
		// These default settings are meant to be overridden by a config file on startup.
		private static ivec2 renderDimensions = new ivec2(900, 506);
		private static ivec2 windowDimensions = new ivec2(900, 506);

		public static int RenderWidth => renderDimensions.x;
		public static int RenderHeight => renderDimensions.y;
		public static int WindowWidth => windowDimensions.x;
		public static int WindowHeight => windowDimensions.y;

		public static ivec2 RenderDimensions
		{
			get => renderDimensions;
			set
			{
				renderDimensions = value;

				MessageSystem.Send(CoreMessageTypes.ResizeRender, value);
			}
		}

		public static ivec2 WindowDimensions
		{
			get => windowDimensions;
			set
			{
				windowDimensions = value;

				MessageSystem.Send(CoreMessageTypes.ResizeWindow, value);
			}
		}
	}
}
