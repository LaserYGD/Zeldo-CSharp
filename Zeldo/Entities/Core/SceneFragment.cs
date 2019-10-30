﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine.Core._3D;
using Engine.Physics;
using Engine.Utility;
using GlmSharp;
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
			var jStatic = json["Static"];

			// All fragments are required to specify a static mesh.
			Debug.Assert(jStatic != null, $"Fragment {filename} missing static block.");

			var jMap = jStatic["Map"];
			var jPhysics = jStatic["Physics"];
			var jOrigin = json["Origin"];
			var jSpawn = json["Spawn"];

			Debug.Assert(jMap != null, $"Fragment {filename} missing static map.");
			Debug.Assert(jPhysics != null, $"Fragment {filename} missing static physics.");
			Debug.Assert(jOrigin != null, $"Fragment {filename} missing origin.");
			Debug.Assert(jSpawn != null, $"Fragment {filename} missing player spawn point.");

			var origin = Utilities.ParseVec3(jOrigin.Value<string>());
			var model = new Model(jMap.Value<string>());
			model.Position = origin;

			var shape = TriangleMeshLoader.Load(jPhysics.Value<string>());
			var body = new RigidBody(shape, RigidBodyTypes.Static);
			body.Position = origin.ToJVector();

			var material = body.Material;
			material.KineticFriction = 0;
			material.StaticFriction = 0;

			var fragment = new SceneFragment(filename);
			fragment.Entities = LoadEntities(json, scene, origin);
			fragment.MapModel = model;
			fragment.MapBody = body;
			fragment.Origin = origin;
			fragment.Spawn = Utilities.ParseVec3(jSpawn.Value<string>());

			return fragment;
		}

		private static Entity[] LoadEntities(JObject json, Scene scene, vec3 origin)
		{
			JArray array = (JArray)json["Entities"];

			// It's valid for a scene to contain no entities (although unlikely in the finished game).
			if (array == null)
			{
				return null;
			}

			var entities = new Entity[array.Count];

			for (int i = 0; i < array.Count; i++)
			{
				var block = array[i];
				var type = Type.GetType("Zeldo.Entities." + block["Type"].Value<string>());

				Debug.Assert(type != null, $"Missing entity type {type.FullName}.");

				// Position will almost always be given, but can be omitted for certain entities (like rope bridges,
				// which instead specify endpoints).
				var jPosition = block["Position"];
				var p = jPosition != null ? Utilities.ParseVec3(jPosition.Value<string>()) : vec3.Zero;

				Entity entity = (Entity)Activator.CreateInstance(type);

				// Position is intentionally set before initialization so that entities can reuse that position as a
				// custom origin if needed.
				entity.Position = origin + p;
				entity.Initialize(scene, block);
				entities[i] = entity;
			}

			// This is used for the debug assertion below.
			var ids = new HashSet<int>();

			foreach (var entity in entities)
			{
				var id = entity.Id;

				if (id == -1)
				{
					continue;
				}

				if (ids.Contains(id))
				{
					Debug.Fail($"Duplicate entity ID ({id}).");
				}

				ids.Add(id);
			}

			return entities;
		}

		private SceneFragment(string filename)
		{
			Filename = filename;
		}

		// This is used for asserting that the fragment aren't loaded multiple times.
		public string Filename { get; }

		public Entity[] Entities { get; private set; }
		public Model MapModel { get; private set; }
		public RigidBody MapBody { get; private set; }

		// This is the default player spawn within the fragment. Only applicable if loading into that fragment
		// directly (used frequently during development).
		public vec3 Spawn { get; private set; }
		public vec3 Origin { get; private set; }
	}
}
