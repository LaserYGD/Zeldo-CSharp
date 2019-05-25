using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;

namespace Engine.Core
{
	public abstract class DynamicComponent : IDynamic
	{
		public abstract bool Complete { get; }

		public abstract void Update(float dt);
	}
}
