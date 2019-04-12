using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.View;

namespace Engine.Entities
{
	public class Scene : IDynamic, IRenderable3D
	{
		private List<Entity3D> entities;

		public Scene(Camera3D camera)
		{
			Camera = camera;
			entities = new List<Entity3D>();
		}

		public Camera3D Camera { get; }

		public void Add(Entity3D entity)
		{
			entities.Add(entity);
			entity.Scene = this;
		}

		public List<T> GetEntityList<T>() where T : Entity3D
		{
			return null;
		}

		public void Update(float dt)
		{
			entities.ForEach(e => e.Update(dt));
		}

		public void Draw(Camera3D camera)
		{
			entities.ForEach(e => e.Draw(camera));
		}
	}
}
