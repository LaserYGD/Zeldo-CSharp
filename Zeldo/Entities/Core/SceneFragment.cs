using Engine;
using Engine.Core._3D;
using Engine.Utility;
using Jitter.Dynamics;
using Newtonsoft.Json.Linq;
using Zeldo.Physics;

namespace Zeldo.Entities.Core
{
	public class SceneFragment
	{
		private static EntityFactory entityFactory = new EntityFactory();

		public static SceneFragment Load(string filename)
		{
			var json = JsonUtilities.Load("Fragments/" + filename);
			var mapToken = json["Map"];

			// If a map isn't given explicitly, it's assumed to use the same name as the fragment file (with a .obj
			// extension).
			string map = mapToken?.Value<string>() ?? filename.StripExtension() + ".obj";

			/*
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
			*/
			
			var entities = ParseEntities(json);
			
			return new SceneFragment(map, entities);
		}

		private static Entity[] ParseEntities(JObject json)
		{
			JArray array = (JArray)json["Entities"];

			if (array == null)
			{
				return null;
			}

			var entities = new Entity[array.Count];

			for (int i = 0; i < array.Count; i++)
			{
				var block = array[i];

				string type = block["Type"].Value<string>();
				string position = block["Position"].Value<string>();

				Entity entity = entityFactory.Create(type);
				entity.Position = Utilities.ParseVec3(position);
				entities[i] = entity;
			}

			return entities;
		}

		private SceneFragment(string map, Entity[] entities)
		{
			// Each physics mesh is located in the "Physics" folder within the parent folder, and uses "_Physics" in
			// the filename.
			string filename = map.StripPath(out string path).StripExtension();
			string physicsFile = $"{path}/Physics/{filename}_Physics.obj";

			MapModel = new Model(map);
			MapBody = new RigidBody(TriangleMeshLoader.Load(physicsFile));
			MapBody.IsStatic = true;
			Entities = entities;
		}

		public Model MapModel { get; }
		public RigidBody MapBody { get; }
		public Entity[] Entities { get; }
	}
}
