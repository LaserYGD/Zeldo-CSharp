using Engine;

namespace Zeldo.Entities
{
	public class PlayerData
	{
		public PlayerData()
		{
			JumpSpeed = Properties.GetFloat("player.jump.speed");
			AerialAttackBoost = Properties.GetFloat("player.aerial.attack.boost");
		}

		public float JumpSpeed { get; }
		public float AerialAttackBoost { get; }
	}
}
