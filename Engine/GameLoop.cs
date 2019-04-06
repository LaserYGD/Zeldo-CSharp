using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;

namespace Engine
{
	public abstract class GameLoop : IDynamic
	{
		public abstract void Initialize();
		public abstract void Update(float dt);
	}
}
