using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Input.Data;
using Engine.Interfaces;

namespace Engine.Input
{
	public class InputBuffer : IComponent
	{
		// Input buffers are designed to be conceptually bound to actions (i.e. lists of binds), not a specific bind.
		// That said, the buffer is considered satisfied if the *same* bind is 
		private Dictionary<InputBind, BindTuple> map;

		private float duration;
		
		private bool requiresHold;

		// Some buffered actions require the bind to still be held when the action occurs, while others don't.
		public InputBuffer(float duration, bool requiresHold, List<InputBind> binds)
		{
			this.duration = duration;
			this.requiresHold = requiresHold;

			map = new Dictionary<InputBind, BindTuple>();
			Binds = binds;
		}
		
		// Buffers are designed to be persistent (i.e. repeatable).
		public bool IsComplete => false;
		public bool IsSatisfied => map.Values.Any(t => t.IsPaused);

		public List<InputBind> Binds
		{
			set
			{
				Debug.Assert(value != null, "Can't set null binds on an input buffer.");

				map.Clear();

				value.ForEach(b => map.Add(b, new BindTuple()));
			}
		}

		public void OnPress(InputBind bind)
		{
			Debug.Assert(map.Keys.Contains(bind), "Attemping to start an input buffer for an invalid bind " +
				$"('{bind}'). This likely means the buffer's bind list wasn't updated correctly.");

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

		public void OnRelease(InputBind bind)
		{
			Debug.Assert(map.Keys.Contains(bind), "Attemping to release an input buffer for an invalid bind " +
				$"('{bind}'). This likely means the buffer's bind list wasn't updated correctly.");

			if (requiresHold)
			{
				var tuple = map[bind];
				tuple.Elapsed = 0;
				tuple.IsPaused = true;
			}
		}

		public void Update(float dt)
		{
			foreach (var tuple in map.Values)
			{
				tuple.Elapsed += dt;

				if (tuple.Elapsed >= duration)
				{
					tuple.Elapsed = 0;
					tuple.IsPaused = true;
				}
			}
		}

		private class BindTuple
		{
			public bool IsPaused { get; set; }
			public float Elapsed { get; set; }
		}
	}
}
