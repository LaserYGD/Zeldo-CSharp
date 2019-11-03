using System;

namespace Engine.Timing
{
	[Flags]
	public enum TimerFlags
	{
		None = 0,
		IsPaused = 1<<0,
		IsRepeatable = 1<<1
	}
}
