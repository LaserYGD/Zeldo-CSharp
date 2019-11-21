using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Engine.Core;
using Engine.Interfaces;

namespace Engine.Props
{
	public static class Properties
	{
		private static Dictionary<string, string> map = new Dictionary<string, string>();
		private static PropertyAccessor accessor;

		// This function (and the one below) are used directly as terminal command functions.
		internal static bool TryEcho(string[] args, out string result)
		{
			if (args.Length != 1)
			{
				result = "Usage: echo *key*";

				return false;
			}

			var key = args[0];

			if (!map.TryGetValue(key, out result))
			{
				result = $"Unknown property '{key}'.";

				return false;
			}

			return true;
		}

		internal static bool TryModify(string[] args, out string result)
		{
			if (args.Length != 2)
			{
				result = "Usage: set *key* *value*";

				return false;
			}

			var key = args[0];

			if (!map.ContainsKey(key))
			{
				result = $"Unknown property '{key}'.";

				return false;
			}

			var tracker = accessor.Tracker;
			var matchedProperty = tracker.Keys.FirstOrDefault(prop => prop.Key == key);

			if (matchedProperty == null)
			{
				result = $"Property '{key}' is untracked.";

				return false;
			}

			var targets = tracker[matchedProperty];
			var value = args[1];

			if (!matchedProperty.Test(value))
			{
				result = $"Value '{value}' is not a valid {matchedProperty.Type.ToString().Uncapitalize()}.";

				return false;
			}

			map[key] = value;
			targets.ForEach(t => t.Reload(accessor));
			result = $"Property '{key}' modified (and targets reloaded).";

			return true;
		}

		public static void Reload()
		{
			map = new Dictionary<string, string>();

			foreach (var file in Directory.GetFiles(Paths.Properties))
			{
				Reload(file);
			}
		}

		private static void Reload(string filename)
		{
			Debug.Assert(File.Exists(filename), $"Missing property file '{filename.StripPath()}'.");

			var lines = File.ReadAllLines(filename);

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];

				// Comments start with '#'.
				if (line.Length == 0 || line[0] == '#')
				{
					continue;
				}

				Debug.Assert(!line.StartsWith("//"), $"Invalid property ('{filename}', line {i}'): Use # for comments.");
				Debug.Assert(!line.EndsWith(";"), $"Invalid property ('{filename}', line {i}'): Don't end lines with semicolons.");

				// The expected format of each line is "key = value" (although it'll work without the spaces as well).
				string[] tokens = line.Split('=');

				Debug.Assert(tokens.Length == 2, $"Invalid property ('{filename}', line {i}'): Expected format is 'key = value'.");

				string key = tokens[0].TrimEnd();
				string value = tokens[1].TrimStart();

				// This assumes that no two properties will share the same name (across all loaded property files).
				map.Add(key, value);
			}

			accessor = new PropertyAccessor(map);
		}

		public static PropertyAccessor Access()
		{
			return accessor;
		}

		public static PropertyAccessor Access(IReloadable target)
		{
			target.Reload(accessor);

			// Returning the accessor directly allows targets to also retrieve any non-reloadable values.
			return accessor;
		}

		public static void Remove(IReloadable target)
		{
		}
	}
}
