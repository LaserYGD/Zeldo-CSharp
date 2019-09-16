using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Engine.Utility
{
	public static class JsonUtilities
	{
		public static JObject Load(string filename)
		{
			Debug.Assert(File.Exists(Paths.Json + filename), $"Missing Json file '{filename}'.");

			return JObject.Parse(File.ReadAllText(Paths.Json + filename));
		}

		public static T Deserialize<T>(string filename, bool useTypes = false)
		{
			Debug.Assert(File.Exists(Paths.Json + filename), $"Missing Json file '{filename}'.");

			var raw = File.ReadAllText(Paths.Json + filename);

			if (!useTypes)
			{
				return JsonConvert.DeserializeObject<T>(raw);
			}

			var settings = new JsonSerializerSettings();
			settings.TypeNameHandling = TypeNameHandling.Auto;

			return JsonConvert.DeserializeObject<T>(raw, settings);
		}
	}
}
