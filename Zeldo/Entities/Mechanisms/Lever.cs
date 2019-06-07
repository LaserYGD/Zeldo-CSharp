using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Mechanisms
{
	public class Lever : Entity, IInteractive
	{
		private bool switchedOn;

		public Lever() : base(EntityGroups.Mechanism)
		{
		}

		public bool IsInteractionEnabled => true;

		public void OnInteract(Entity entity)
		{
		}
	}
}
