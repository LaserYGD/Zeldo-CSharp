using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.View;
using Zeldo.Sensors;

namespace Zeldo.Entities.Core
{
	public class Scene : IDynamic, IRenderable3D
	{
		private List<Entity> entities;

		public Scene()
		{
			entities = new List<Entity>();
		}

		public Camera3D Camera { get; set; }
		public Space Space { get; set; }

		public void Add(Entity entity)
		{
			entities.Add(entity);
			entity.Scene = this;
		}

		public List<Entity> GetEntityList(EntityTypes type)
		{
			return null;
		}

		public void Update(float dt)
		{
			entities.ForEach(e => e.Update(dt));
		}

		public void Draw(Camera3D camera)
		{
		}
	}
}
