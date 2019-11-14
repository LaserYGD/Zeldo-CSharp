using Zeldo.Entities.Core;

namespace Zeldo.Entities.Weapons
{
	public abstract class MeleeWeapon<T> : Weapon<T> where T : Actor
	{
		protected MeleeWeapon(string attackFile, T owner) : base(attackFile, owner)
		{
		}
	}
}
