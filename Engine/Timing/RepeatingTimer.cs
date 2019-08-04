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

		public override void Update(float dt)
		{
			if (Paused)
			{
				return;
			}

			Elapsed += dt;

			while (Elapsed >= Duration && !Paused)
			{
				float previousDuration = Duration;

				if (!trigger(Elapsed % Duration))
				{
					if (Repeatable)
					{
						Elapsed = 0;
						Paused = true;
					}
					else
					{
						IsComplete = true;
					}

					return;
				}

				Elapsed -= previousDuration;
			}

			Tick?.Invoke(Elapsed / Duration);
		}
	}
}
