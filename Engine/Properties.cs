using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Engine
{
	public static class Properties
	{
		private static Dictionary<string, string> map = new Dictionary<string, string>();

		public static void LoadAll()
		{
			foreach (var file in Directory.GetFiles(Paths.Properties))
			{
				Load(file);
			}
		}

		private static void Load(string filename)
		{
			Debug.Assert(File.Exists(filename), $"Missing property file '{filename.StripPath()}'.");

			string[] lines = File.ReadAllLines(filename);

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
		}

		public static int GetInt(string key)
		{
			Debug.Assert(map.ContainsKey(key), $"Missing property '{key}'.");

			return int.Parse(map[key]);
		}

		public static float GetFloat(string key)
		{
			Debug.Assert(map.ContainsKey(key), $"Missing property '{key}'.");

			return float.Parse(map[key]);
		}

		public static string GetString(string key)
		{
			Debug.Assert(map.ContainsKey(key), $"Missing property '{key}'.");

			return map[key];
		}
	}
}
