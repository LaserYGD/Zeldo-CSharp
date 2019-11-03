using System;
using System.Diagnostics;

namespace Engine.Timing
{
	public class RepeatingTimer : Timer
	{
		private Func<float, bool> trigger;

		// Just like single timers, repeatable timers are repeatable by default (and start paused).
		public RepeatingTimer(Func<float, bool> trigger, float duration = 0,
			TimerFlags flags = TimerFlags.IsPaused | TimerFlags.IsRepeatable, float elapsed = 0) :
			base(duration, elapsed, flags)
		{
			this.trigger = trigger;
		}

		// In some cases, it's useful to access progress outside of the tick function.
		public float Progress => Elapsed / Duration;

		public override void Update(float dt)
		{
			if (IsPaused || IsComplete)
			{
				return;
			}

			Debug.Assert(duration > 0, "Can't update a timer with a non-positive duration.");

			elapsed += dt;

			while (elapsed >= duration && !IsPaused)
			{
				float previousDuration = duration;

				// If the trigger function is null, the repeating timer ends (otherwise you'd be stuck in an infinite
				// loop).
				if (trigger == null || !trigger.Invoke(elapsed % duration))
				{
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

				elapsed -= previousDuration;
			}

			Tick?.Invoke(Progress);
		}
	}
}
