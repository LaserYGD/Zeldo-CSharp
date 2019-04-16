using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeldo.Entities.Core;

namespace Zeldo.Interfaces
{
	public interface IInteractive
	{
		bool InteractionEnabled { get; }

		void OnInteract(Entity entity);
	}
}
