using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Core;
using Engine.Interfaces;

namespace Engine.Props
{
	public class PropertyAccessor
	{
		private Dictionary<string, string> map;
		private Dictionary<Property, List<IReloadable>> tracker;

		internal PropertyAccessor(Dictionary<string, string> map)
		{
			this.map = map;

			tracker = new Dictionary<Property, List<IReloadable>>();
		}

		internal Dictionary<Property, List<IReloadable>> Tracker => tracker;

		public int GetInt(string key, IReloadable target = null, bool shouldTrack = true)
		{
			Validate(key, PropertyTypes.Integer, target, shouldTrack);

			if (!int.TryParse(map[key], out var result))
			{
				Debug.Fail($"Property '{key}={result}' is not a valid integer.");
			}

			return result;
		}

		public float GetFloat(string key, IReloadable target = null, bool shouldTrack = true)
		{
			Validate(key, PropertyTypes.Float, target, shouldTrack);

			if (!float.TryParse(map[key], out var result))
			{
				Debug.Fail($"Property '{key}={result}' is not a valid float.");
			}

			return result;
		}

		public string GetString(string key, IReloadable target = null, bool shouldTrack = true)
		{
			Validate(key, PropertyTypes.String, target, shouldTrack);

			return map[key];
		}

		public Color GetColor(string key, IReloadable target = null, bool shouldTrack = true)
		{
			Validate(key, PropertyTypes.Color, target, shouldTrack);

			if (!Color.TryParse(map[key], out var result))
			{
				Debug.Fail($"Property '{key}={result}' is not a valid color.");
			}

			return result;
		}

		private void Validate(string key, PropertyTypes type, IReloadable target, bool shouldTrack)
		{
			Debug.Assert(map.ContainsKey(key), $"Missing property '{key}'.");

			// This is useful for properties that should never be iterated during gameplay (or where live modification
			// would be too cumbersome or error-prone).
			if (target == null || !shouldTrack)
			{
				return;
			}

			var matchedProperty = tracker.Keys.FirstOrDefault(p => p.Key == key);

			if (matchedProperty == null)
			{
				var list = new List<IReloadable>();
				list.Add(target);

				tracker.Add(new Property(key, type), list);
			}
			else
			{
				Debug.Assert(type == matchedProperty.Type, $"Type conflict for property '{key}' (was previously " +
					$"retrieved as {matchedProperty.Type}, but is now {type}).");

				var targets = tracker[matchedProperty];

				// It's fine for the same object accesses the same property multiple times (this most commonly occurs
				// when a property is modified, triggering a reload).
				if (!targets.Contains(target))
				{
					targets.Add(target);
				}
			}
		}
	}
}
