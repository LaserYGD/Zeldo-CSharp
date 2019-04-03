using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Entities;

namespace Engine.Interfaces
{
	public interface IInteractive
	{
		void OnInteract(Entity3D entity);
	}
}
