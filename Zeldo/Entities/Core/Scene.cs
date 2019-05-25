﻿using System.Collections.Generic;
using Engine.Graphics._3D;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.UI;
using Engine.View;
using Jitter;
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
		}

		public Camera3D Camera { get; set; }
		public Canvas Canvas { get; set; }
		public Space Space { get; set; }
		public World World { get; set; }
		public ModelBatch ModelBatch { get; }

		public void LoadFragment(string filename)
		{
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
