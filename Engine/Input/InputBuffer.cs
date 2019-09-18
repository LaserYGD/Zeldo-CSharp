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
		
		private bool requiresHold;

		// Some buffered actions require the bind to still be held when the action occurs, while others don't.
		public InputBuffer(float duration, bool requiresHold, List<InputBind> binds)
		{
			Debug.Assert(duration > 0, "Input buffer duration must be positive.");

			this.duration = duration;
			this.requiresHold = requiresHold;

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

		public bool Refresh(FullInputData data, float dt)
		{
			foreach (var bind in map.Keys)
			{
				var state = data[bind];

				if (state == InputStates.PressedThisFrame)
				{
					OnPress(bind);
				}
				else if (requiresHold && state == InputStates.ReleasedThisFrame)
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
				tuple.Elapsed = 0;
			}
			else
			{
				tuple.IsPaused = false;
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
