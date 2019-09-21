using System.Collections.Generic;

namespace Engine
{
	public static class Statistics
	{
		private static Dictionary<string, int> map = new Dictionary<string, int>();
		private static List<string> sortedKeys = new List<string>();

		// Using a temporary lock prevents the debug text itself from affecting collected stats.
		private static bool isLocked;

		public static (string Key, int Value)[] Enumerate()
		{
			var results = new (string Key, int Value)[sortedKeys.Count];

			for (int i = 0; i < sortedKeys.Count; i++)
			{
				var k = sortedKeys[i];
				results[i] = (k, map[k]);
			}

			return results;
		}

		public static void Increment(string key, int value = 1)
		{
			if (isLocked)
			{
				return;
			}

			if (!map.ContainsKey(key))
			{
				map.Add(key, 0);

				// For render statistics, it's likely that few enough keys will be added that the full list can be
				// easily sorted with each new addition.
				sortedKeys.Add(key);
				sortedKeys.Sort();
			}

			map[key] += value;
		}

		public static void Lock()
		{
			isLocked = true;
		}

		public static void Reset()
		{
			foreach (var key in sortedKeys)
			{
				map[key] = 0;
			}

			isLocked = false;
		}
	}
}
