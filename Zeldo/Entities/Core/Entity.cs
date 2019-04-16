using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Zeldo.Entities.Core
{
	public abstract class Entity : ITransformable3D, IDynamic
	{
		private List<DynamicComponent> components;

		protected Entity(EntityTypes type)
		{
			EntityType = type;
		}

		protected List<DynamicComponent> Components => components ?? (components = new List<DynamicComponent>());

		public EntityTypes EntityType { get; }

		public Scene Scene { get; set; }
		public vec3 Position { get; set; }
		public quat Orientation { get; set; }

		public virtual void Initialize()
		{
		}

		public void SetTransform(vec3 position, quat orientation)
		{
			Position = position;
			Orientation = orientation;
		}

		public virtual void Update(float dt)
		{
			if (components == null)
			{
				return;
			}

			for (int i = components.Count - 1; i >= 0; i--)
			{
				var component = components[i];
				component.Update(dt);

				if (component.Complete)
				{
					components.RemoveAt(i);
				}
			}
		}
	}
}
