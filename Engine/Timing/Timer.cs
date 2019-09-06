using System;
using Engine.Interfaces;

namespace Engine.Timing
{
	public abstract class Timer : IComponent
	{
		protected Timer(float duration, float elapsed = 0)
		{
			Elapsed = elapsed;
			Duration = duration;
		}

		public float Elapsed { get; set; }
		public float Duration { get; set; }

		public bool IsPaused { get; set; }
		public bool IsRepeatable { get; set; }
		public bool IsComplete { get; protected set; }

		public Action<float> Tick { get; set; }

		public abstract void Update(float dt);
	}
}
