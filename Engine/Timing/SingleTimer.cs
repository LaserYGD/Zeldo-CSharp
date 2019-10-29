using System;
using System.Diagnostics;

namespace Engine.Timing
{
	public class SingleTimer : Timer
	{
		public SingleTimer(Action<float> trigger = null, float duration = 0, float elapsed = 0) :
			base(duration, elapsed)
		{
			Trigger = trigger;
		}

		// The argument is the leftover time (since the duration is unlikely to be hit exactly).
		public Action<float> Trigger { get; set; }

		public override void Update(float dt)
		{
			if (IsPaused || IsComplete)
			{
				return;
			}

			Debug.Assert(duration > 0, "Can't update a timer with a non-positive duration.");

			elapsed += dt;

			if (elapsed >= duration)
			{
				// Calling the tick function here means that tick logic doesn't need to be duplicated in the trigger
				// function.
				Tick?.Invoke(1);

				// It's considered valid for the trigger function to be null (primarly for tick-only timers).
				Trigger?.Invoke(elapsed - duration);

				if (IsRepeatable)
				{
					Elapsed = 0;
					IsPaused = true;
				}
				else
				{
					IsComplete = true;
				}

				return;
			}

			Tick?.Invoke(elapsed / duration);
		}
	}
}
