using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Entities
{
	public class Scene
	{
		private List<Entity3D> entities;

		public Scene()
		{
			entities = new List<Entity3D>();
		}

		public void Add(Entity3D entity)
		{
		}

		public List<T> GetEntityList<T>() where T : Entity3D
		{
			return null;
		}
	}
}
