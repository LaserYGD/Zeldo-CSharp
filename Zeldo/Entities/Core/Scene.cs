﻿using System.Collections.Generic;
using System.Linq;
using Engine.Graphics._3D;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.UI;
using Engine.Utility;
using Engine.View;
using Jitter;
using Newtonsoft.Json.Linq;
using Zeldo.Sensors;

namespace Zeldo.Entities.Core
{
	public class Scene : IDynamic, IRenderable3D
	{
		private List<Entity> entities;

		public Scene()
		{
			entities = new List<Entity>();
			ModelBatch = new ModelBatch(200000, 20000);
			UserData = new Dictionary<string, object>();
		}

		public Camera3D Camera { get; set; }
		public Canvas Canvas { get; set; }
		public Space Space { get; set; }
		public World World { get; set; }
		public ModelBatch ModelBatch { get; }

		// In this context, "user data" means custom data optionally loaded with each fragment. Used as needed in order
		// to implement custom features for different kinds of locations.
		public Dictionary<string, object> UserData { get; }

		public void LoadFragment(string filename)
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
		}

		public void Add(Entity entity)
		{
			entities.Add(entity);
			entity.Initialize(this);
		}

		public void Remove(Entity entity)
		{
			entity.Dispose();
			entities.Remove(entity);
		}

		public void Update(float dt)
		{
			entities.ForEach(e => e.Update(dt));
		}

		public void Draw(Camera3D camera)
		{
			ModelBatch.ViewProjection = camera.ViewProjection;
			ModelBatch.Draw();
		}
	}
}
