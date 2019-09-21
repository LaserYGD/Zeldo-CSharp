using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Input.Data;

namespace Engine.Input
{
	public class InputBuffer
	{
		// Input buffers are designed to be conceptually bound to actions (i.e. lists of binds), not a specific bind.
		// That said, the buffer is considered satisfied if the *same* bind is 
		private Dictionary<InputBind, BindTuple> map;

		private float duration;
		
		// Some buffered actions require the bind to still be held when the action occurs, while others don't.
		public InputBuffer(float duration, bool requiresHold, List<InputBind> binds)
		{
			Debug.Assert(duration > 0, "Input buffer duration must be positive.");

			this.duration = duration;

			RequiresHold = requiresHold;
			map = new Dictionary<InputBind, BindTuple>();
			Binds = binds;
		}

		public List<InputBind> Binds
		{
			set
			{
				Debug.Assert(value != null, "Can't set null binds on an input buffer.");

				map.Clear();

				value.ForEach(b => map.Add(b, new BindTuple()));
			}
		}

		// TODO: Is a more robust chord system needed for input buffers?
		// This allows actions to be activated only when another bind is held (e.g. ascend, which requires holding
		// another key when jump is pressed near an ascension target). For the time being, this works the same as Ori's
		// charge dash logic, where pressing both binds on the same frame also counts as a successful activation.
		public List<InputBind> RequiredChords { get; set; }

		public bool RequiresHold { get; set; }

		public bool Refresh(FullInputData data, float dt)
		{
			foreach (var bind in map.Keys)
			{
				var state = data[bind];

				if (state == InputStates.PressedThisFrame)
				{
					bool isChordSatisfied = RequiredChords == null || data.Query(RequiredChords, InputStates.Held);

					// TODO: Probably worth revisiting how chorded buffers are handled (to make sure they apply to all use cases).
					// By design, required chords are designed to work as modifiers on the base binds. In other words,
					// the action only triggers if a bind is pressed *while one of the chords is held*. This also means
					// that the chord could be released by the time the buffered action occurs, but I think that's
					// probably fine.
					if (isChordSatisfied)
					{
						OnPress(bind);
					}
				}
				// If a required chord is set, it's possible to press a bind, then hold the chord, then release the
				// bind. This could trigger a false release without verifying the bind was actually pressed first (with
				// the chord), which is determined via IsPaused.
				else if (RequiresHold && state == InputStates.ReleasedThisFrame && !map[bind].IsPaused)
				{
					OnRelease(bind);
				}
				else
				{
					var tuple = map[bind];
					tuple.Elapsed += dt;

					if (tuple.Elapsed >= duration)
					{
						tuple.Elapsed = 0;
						tuple.IsPaused = true;
					}
				}
			}

			return map.Values.Any(t => !t.IsPaused);
		}

		public bool Refresh(FullInputData data, float dt, out InputBind bind)
		{
			bind = null;

			if (Refresh(data, dt))
			{
				float min = float.MaxValue;

				foreach (var key in map.Keys)
				{
					var tuple = map[key];

					// If multiple binds were successfully buffered, the bind that was pressed most recently is
					// returned.
					if (!tuple.IsPaused && tuple.Elapsed < min)
					{
						min = tuple.Elapsed;
						bind = key;
					}
				}
			}

			return bind != null;
		}

		private void OnPress(InputBind bind)
		{
			var tuple = map[bind];

			if (tuple.IsPaused)
			{
				tuple.IsPaused = false;
			}
			else
			{
				tuple.Elapsed = 0;
			}
		}

		private void OnRelease(InputBind bind)
		{
			var tuple = map[bind];
			tuple.Elapsed = 0;
			tuple.IsPaused = true;
		}

		private class BindTuple
		{
			public BindTuple()
			{
				IsPaused = true;
			}

			public bool IsPaused { get; set; }
			public float Elapsed { get; set; }
		}
	}
}
