using System.Collections.Generic;
using System.IO;

namespace Engine.Localization
{
	public static class Language
	{
		private static Dictionary<string, string> map = new Dictionary<string, string>();
		private static string missingText;

		public static void Reload(Languages language)
		{
			map.Clear();

			string[] lines = File.ReadAllLines("Content/Language/" + language + ".txt");

			// The first line in each language file is a placeholder string for missing text values.
			missingText = lines[0];

			for (int i = 1; i < lines.Length; i++)
			{
				string line = lines[i];

				if (line.Length == 0)
				{
					continue;
				}

				string[] tokens = line.Split('|');
				string key = tokens[0];

				// If only one token is present (i.e. there's no pipe), the key and value are assumed to be the same.
				// This is a useful shorthand for many English strings.
				string value = tokens.Length == 1 ? key : tokens[1];

				map.Add(key, value);
			}
		}

		public static string GetString(string key)
		{
			return map.TryGetValue(key, out string value) ? value : string.Format($"[{missingText} => \"{key}\"]");
		}
	}
}
