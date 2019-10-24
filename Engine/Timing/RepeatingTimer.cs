using System;

namespace Engine.Timing
{
	public class RepeatingTimer : Timer
	{
		private Func<float, bool> trigger;

		public RepeatingTimer(Func<float, bool> trigger, float duration = 0, float elapsed = 0) :
			base(duration, elapsed)
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

			Elapsed += dt;

			while (Elapsed >= Duration && !IsPaused)
			{
				float previousDuration = Duration;

				if (!trigger(Elapsed % Duration))
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

				Elapsed -= previousDuration;
			}

			Tick?.Invoke(Progress);
		}
	}
}
