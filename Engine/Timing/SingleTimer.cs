using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Timing
{
	public class SingleTimer : Timer
	{
		// The argument is the leftover time (since the duration is unlikely to be hit exactly).
		private Action<float> trigger;

		public SingleTimer(Action<float> trigger, float duration = 0, float elapsed = 0) : base(duration, elapsed)
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

			if (Elapsed >= Duration)
			{
				// Calling the tick function here allows tick logic to not be duplicated in the trigger function.
				Tick?.Invoke(1);
				trigger(Elapsed - Duration);

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

			Tick?.Invoke(Elapsed / Duration);
		}
	}
}
