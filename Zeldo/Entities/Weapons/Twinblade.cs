using Zeldo.Entities.Player;

namespace Zeldo.Entities.Weapons
{
	public class Twinblade : MeleeWeapon<PlayerCharacter>
	{
		public Twinblade(PlayerCharacter owner) : base("PlayerTwinbladeAttacks.json", owner)
		{
		}

		protected override void TriggerPrimary(out float cooldownTime, out float bufferTime)
		{
			cooldownTime = 0.75f;
			bufferTime = 0.4f;
		}
	}
}
