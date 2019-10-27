using System;

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

			Elapsed += dt;

			if (Elapsed >= Duration)
			{
				// Calling the tick function here means that tick logic doesn't need to be duplicated in the trigger
				// function.
				Tick?.Invoke(1);

				// It's considered valid for the trigger function to be null (primarly for tick-only timers).
				Trigger?.Invoke(Elapsed - Duration);

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

			Tick?.Invoke(Elapsed / Duration);
		}
	}
}
