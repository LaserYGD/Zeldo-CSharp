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

			return null;
		}

		public Model MapModel { get; }
	}
}
