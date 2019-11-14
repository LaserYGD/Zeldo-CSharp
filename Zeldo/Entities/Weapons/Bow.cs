using Zeldo.Entities.Player;
using Zeldo.Entities.Player.Combat;

namespace Zeldo.Entities.Weapons
{
	public class Bow : Weapon<PlayerCharacter>
	{
		public Bow(PlayerCharacter owner) : base("PlayerBowAttacks.json", owner)
		{
		}

		public override void ReleasePrimary()
		{
			// TODO: This will have to change when additional bow attacks are added (since not all of them can be held and released).
			((BowShot)ActiveAttack).ShouldRelease = true;
		}
	}
}
