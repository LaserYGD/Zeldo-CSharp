using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics._3D;
using Engine.Interfaces;
using Engine.View;
using Zeldo.Sensors;

namespace Zeldo.Entities.Core
{
	public class Scene : IDynamic
	{
		private List<Entity> entities;

		public Scene()
		{
			entities = new List<Entity>();
			ModelBatch = new ModelBatch(10000, 1000);
		}

		public Camera3D Camera { get; set; }
		public Space Space { get; set; }
		public ModelBatch ModelBatch { get; }

		public void LoadFragment(string filename)
		{
		}

		public void Add(Entity entity)
		{
			entities.Add(entity);
			entity.Scene = this;
			entity.Initialize();
		}

		public List<Entity> GetEntityList(EntityTypes type)
		{
			return null;
		}

		public void Update(float dt)
		{
			entities.ForEach(e => e.Update(dt));
		}
	}
}
