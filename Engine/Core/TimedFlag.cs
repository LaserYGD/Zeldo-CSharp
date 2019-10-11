using System;
using System.Diagnostics;
using Engine.Interfaces;

namespace Engine.Core
{
	public class TimedFlag : IComponent
	{
		private float elapsed;
		private float duration;

		private bool defaultValue;
		private bool isPaused;

		public TimedFlag(float duration, bool defaultValue)
		{
			Debug.Assert(duration > 0, "Timed flag duration must be positive.");

			this.duration = duration;
			this.defaultValue = defaultValue;

			isPaused = true;
		}

		// Flags are designed to be persistent (as long as the parent entity is loaded).
		public bool IsComplete => false;
		public bool Value => isPaused ? !defaultValue : defaultValue;

		public Action OnExpiration { private get; set; }

		public void Refresh()
		{
			if (!isPaused)
			{
				elapsed = 0;
			}
			else
			{
				isPaused = false;
			}
		}

		// This function allows the flag to be reset without trigging the expiration callback.
		public void Reset()
		{
			elapsed = 0;
			isPaused = true;
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
				Reset();
				OnExpiration?.Invoke();
			}
		}
	}
}
