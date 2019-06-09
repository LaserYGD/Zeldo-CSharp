using System;
using System.Collections.Generic;
using Engine;
using Engine.Core._3D;
using Engine.Shapes._2D;
using Engine.Utility;
using GlmSharp;
using Jitter.Dynamics;
using Newtonsoft.Json.Linq;
using Zeldo.Physics;
using Zeldo.Physics._2D;

namespace Zeldo.Entities.Core
{
	public class SceneFragment
	{
		// This map allows fragment files to list rotations exactly (e.g. Pi or PiOverTwo) rather than hardcoding an
		// approximation of those values.
		private static readonly Dictionary<string, float> RotationMap = new Dictionary<string, float>
		{
			{ "Pi", Constants.Pi },
			{ "PiOverTwo", Constants.PiOverTwo },
			{ "PiOverFour", Constants.PiOverFour }
		};

		private static EntityFactory entityFactory = new EntityFactory();

		public static SceneFragment Load(string filename)
		{
			var json = JsonUtilities.Load("Fragments/" + filename);
			var mapToken = json["Map"];

			// If a map isn't given explicitly, it's assumed to use the same name as the fragment file (with a .obj
			// extension, obviously).
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
			var groundBodies = ParseGround(json);

			return new SceneFragment(map, entities, groundBodies);
		}

		private static Entity[] ParseEntities(JObject json)
		{
			JArray array = (JArray)json["Entities"];

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

		private static RigidBody2D[] ParseGround(JObject json)
		{
			JArray array = (JArray)json["Ground"];

			var bodies = new RigidBody2D[array.Count];

			for (int i = 0; i < array.Count; i++)
			{
				var block = array[i];

				string type = block["Type"].Value<string>();

				Shape2D shape = null;

				ShapeTypes2D shapeType = Utilities.EnumParse<ShapeTypes2D>(type);

				switch (shapeType)
				{
					case ShapeTypes2D.Circle:
						float radius = block["Radius"].Value<float>();

						shape = new Circle(radius);

						break;

					case ShapeTypes2D.Line:
						vec2 p1 = Utilities.ParseVec2(block["P1"].Value<string>());
						vec2 p2 = Utilities.ParseVec2(block["P2"].Value<string>());

						shape = new Line2D(p1, p2);

						break;

					case ShapeTypes2D.Rectangle:
						float width = block["Width"].Value<float>();
						float height = block["Height"].Value<float>();

						shape = new Rectangle(width, height);

						break;
				}

				// Most shapes have position, but not all (lines specifically, which use P1 and P2 instead).
				if (shapeType != ShapeTypes2D.Line)
				{
					shape.Position = Utilities.ParseVec2(block["Position"].Value<string>());

					// Neither lines nor circles can be rotated.
					if (shapeType != ShapeTypes2D.Circle)
					{
						var rotationToken = block["Rotation"];

						if (rotationToken != null)
						{
							string value = rotationToken.Value<string>();

							if (!RotationMap.TryGetValue(value, out float rotation))
							{
								rotation = float.Parse(value);
							}

							shape.Rotation = rotation;
						}
					}
				}

				bodies[i] = new RigidBody2D(shape, true);
			}

			return bodies;
		}

		private SceneFragment(string map, Entity[] entities, RigidBody2D[] groundBodies)
		{
			// Each physics mesh is located in the "Physics" folder within the parent folder, and uses "_Physics" in
			// the filename.
			string filename = map.StripPath(out string path).StripExtension();
			string physicsFile = $"{path}/Physics/{filename}_Physics.obj";

			MapModel = new Model(map);
			MapBody = new RigidBody(TriangleMeshLoader.Load(physicsFile));
			MapBody.IsStatic = true;
			Entities = entities;
			GroundBodies = groundBodies;
		}

		public Model MapModel { get; }
		public RigidBody MapBody { get; }
		public RigidBody2D[] GroundBodies { get; }
		public Entity[] Entities { get; }
	}
}
