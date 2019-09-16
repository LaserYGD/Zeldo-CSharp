using Engine.Interfaces;

namespace Engine.Core
{
	public class TimedFlag : IComponent
	{
		private float elapsed;
		private float duration;

		private bool defaultValue;
		private bool isPaused;

		public TimedFlag(float duration, bool value)
		{
			this.duration = duration;

			defaultValue = value;
			isPaused = true;
			Value = value;
		}

		// Flags are designed to be persistent.
		public bool IsComplete => false;
		public bool Value { get; private set; }

		public void Start()
		{
			if (isPaused)
			{
				elapsed = 0;
			}
			else
			{
				isPaused = false;
			}
		}

		public void Update(float dt)
		{
			if (isPaused)
			{
				return;
			}

			elapsed += dt;

			if (elapsed >= duration)
			{
				elapsed = 0;
				Value = defaultValue;
				isPaused = true;
			}
		}
	}
}
