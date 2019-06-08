using System.Linq;
using Engine.Core._3D;
using Engine.Utility;
using Newtonsoft.Json.Linq;

namespace Zeldo.Entities.Core
{
	public class SceneFragment
	{
		public static SceneFragment Load(string filename)
		{
			var json = JsonUtilities.Load("Fragments/" + filename);

			string map = json["Map"].Value<string>();

			var userData = json["UserData"];

			if (userData != null)
			{
				foreach (JProperty block in userData.ToArray())
				{
					var value = block.Value;

					if (value.Type == JTokenType.Array)
					{
						foreach (var item in value)
						{
						}
					}
				}
			}

			return new SceneFragment(map);
		}

		private SceneFragment(string map)
		{
			MapModel = new Model("Maps/" + map);
		}

		public Model MapModel { get; }
	}
}
