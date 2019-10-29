using System;
using System.Diagnostics;
using Engine.Interfaces;

namespace Engine.Timing
{
	public abstract class Timer : IComponent
	{
		protected float elapsed;
		protected float duration;

		protected Timer(float duration, float elapsed = 0)
		{
			Duration = duration;
			Elapsed = elapsed;
		}

		public float Elapsed
		{
			get => elapsed;
			set
			{
				Debug.Assert(value >= 0, "Elapsed time can't be negative.");
				Debug.Assert(value <= Duration, "Elapsed time can't be greater than duration.");

				elapsed = value;
			}
		}

		public float Duration
		{
			get => duration;
			set
			{
				Debug.Assert(value >= 0, "Duration can't be negative.");
				Debug.Assert(value >= elapsed, "Duration can't be less than the current elapsed time.");

				duration = value;
			}
		}

		public bool IsPaused { get; set; }
		public bool IsRepeatable { get; set; }
		public bool IsComplete { get; protected set; }

		public Action<float> Tick { get; set; }

		public abstract void Update(float dt);
	}
}
