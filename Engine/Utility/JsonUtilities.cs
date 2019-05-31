using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Engine.Utility
{
	public static class JsonUtilities
	{
		private const string Path = "Content/Json/";

		public static JObject Load(string filename)
		{
			return JObject.Parse(File.ReadAllText(Path + filename));
		}

		public static T Deserialize<T>(string filename)
		{
			return JsonConvert.DeserializeObject<T>(File.ReadAllText(Path + filename));
		}
	}
}
