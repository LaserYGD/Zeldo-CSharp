using System.Collections.Generic;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Weapons
{
	public abstract class MeleeWeapon : Weapon
	{
		// Melee weapons only hit targets once per swing (even if sensors overlap for multiple frames).
		private List<ITargetable> targetsHit;
	}
}
