using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeldo.Entities.Core
{
	public enum EntityGroups
	{
		Character,
		Enemy,

		// TODO: Consider removing this group if a separate entity class is created for UI-based elements.
		Interface,
		Mechanism,
		Object,
		Player,
		Projectile,
		Weapon
	}
}
