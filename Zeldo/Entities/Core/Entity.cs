using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Zeldo.Entities.Core
{
	public abstract class Entity : ITransformable3D, IDynamic
	{
		protected Entity(EntityTypes type)
		{
			EntityType = type;
		}

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
		}
	}
}
