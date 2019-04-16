using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities
{
	public class TreasureChest : Entity, IInteractive
	{
		private int itemId;
		private bool opened;

		public TreasureChest() : base(EntityTypes.Object)
		{
		}

		public bool InteractionEnabled => !opened;

		public void OnInteract(Entity entity)
		{
			Player player = (Player)entity;
			player.GiveItem(itemId);

			opened = true;
		}
	}
}
