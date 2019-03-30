using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Entities
{
	public class Entity3D : IPositionable3D, IDynamic
	{
		public vec3 Position { get; set; }
		public Scene Scene { get; set; }

		public virtual void Update(float dt)
		{
		}
	}
}
