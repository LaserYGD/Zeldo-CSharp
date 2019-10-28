using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeldo.Entities.Core
{
	// TODO: Consider whether entity groups are needed at all (likely used to more efficiently query entities).
	public enum EntityGroups
	{
		Boss,
		Character,
		Enemy,

		// TODO: Consider removing this group if a separate entity class is created for UI-based elements.
		Interface,
		Mechanism,
		Object,
		Player,
		Platform,
		Projectile,
		Structure,
		Weapon
	}
}
