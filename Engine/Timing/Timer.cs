using System;
using Engine.Core;

namespace Engine.Timing
{
	public abstract class Timer : Component
	{
		protected Timer(float duration, float elapsed = 0)
		{
			Elapsed = elapsed;
			Duration = duration;
		}

		public float Elapsed { get; set; }
		public float Duration { get; set; }

		public bool Paused { get; set; }
		public bool Repeatable { get; set; }

		public Action<float> Tick { get; set; }
	}
}
