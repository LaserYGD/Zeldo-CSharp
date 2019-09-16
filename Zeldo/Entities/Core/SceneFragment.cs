using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Engine;
using Engine.Core._3D;
using Engine.Graphics._3D;
using Engine.Utility;
using Jitter.Dynamics;
using Newtonsoft.Json.Linq;
using Zeldo.Physics;

namespace Zeldo.Entities.Core
{
	public class SceneFragment
	{
		public static SceneFragment Load(string filename, Scene scene)
		{
			var json = JsonUtilities.Load("Fragments/" + filename);
			var entities = LoadEntities(json, scene);
			var staticTokens = (JArray)json["Static"];

			// All fragments are required to have at least one static mesh.
			Debug.Assert(staticTokens != null && staticTokens.Count > 0, $"Fragment '{filename}' needs at least one static mesh.");

			Mesh[] staticMeshes = new Mesh[staticTokens.Count];

			for (int i = 0; i < staticTokens.Count; i++)
			{
				staticMeshes[i] = ContentCache.GetMesh(staticTokens[i].Value<string>());
			}
			
			return new SceneFragment(entities, staticMeshes);
		}

		private static Entity[] LoadEntities(JObject json, Scene scene)
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

				Debug.Assert(type != null, "Entity in fragment file missing type.");
				Debug.Assert(position != null, "Entity in fragment file missing position.");
				Debug.Assert(position.Split('|').Length == 3, "Entity position in wrong format (should be pipe-separated).");

				Entity entity = (Entity)Activator.CreateInstance(Type.GetType("Zeldo.Entities." + type));
				entity.Initialize(scene, block);
				entity.Position = Utilities.ParseVec3(position);
				entities[i] = entity;
			}

			return entities;
		}

		private static Model[] LoadStaticModels(JObject json)
		{
			return null;
		}

		private SceneFragment(Entity[] entities, Model[] staticMeshes)
		{
			// Each physics mesh is located in the "Physics" folder within the parent folder, and uses "_Physics" in
			// the filename.
			string filename = map.StripPath(out string path).StripExtension();
			string physicsFile = $"{path}/Physics/{filename}_Physics.obj";

			Debug.Assert(File.Exists("Content/Meshes/" + ));

			MapModel = new Model(map);
			MapBody = new RigidBody(TriangleMeshLoader.Load(physicsFile));
			MapBody.IsStatic = true;
			Entities = entities;
		}

		public Entity[] Entities { get; }
		public Model[] MapModels { get; }
		public RigidBody[] MapBodies { get; }
	}
}
