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

		// This constructor is useful when duration isn't known on construction.
		public TimedFlag(bool defaultValue = false) : this(0, defaultValue)
		{
		}

		public TimedFlag(float duration, bool defaultValue = false)
		{
			Debug.Assert(duration >= 0, "Timed flag duration can't be negative.");

			this.duration = duration;
			this.defaultValue = defaultValue;

			isPaused = true;
		}

		// Flags are designed to be persistent (as long as the parent entity is loaded).
		public bool IsComplete => false;
		public bool Value => isPaused ? defaultValue : !defaultValue;

		public float Duration
		{
			get => duration;
			set => duration = value;
		}

		// For flags that require tracking data, it often makes sense to track that data within the flag itself.
		public object Tag { get; set; }

		public Action OnExpiration { private get; set; }

		public void Refresh(object tag = null)
		{
			Tag = tag;

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
