using Zeldo.Entities.Player;

namespace Zeldo.Entities.Weapons
{
	public class Twinblade : MeleeWeapon<PlayerCharacter>
	{
		public Twinblade(PlayerCharacter owner) : base("PlayerTwinbladeAttacks.json", owner)
		{
		}
	}
}
