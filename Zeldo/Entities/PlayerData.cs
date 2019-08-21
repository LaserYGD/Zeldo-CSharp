using Engine;

namespace Zeldo.Entities
{
	public class PlayerData
	{
		public PlayerData()
		{
			JumpSpeed = Properties.GetFloat("player.jump.speed");
			SlideThreshold = Properties.GetFloat("player.slide.threshold");
			UpperWallThreshold = Properties.GetFloat("player.upper.wall.threshold");
			LowerWallThreshold = Properties.GetFloat("player.lower.wall.threshold");
			AerialAttackBoost = Properties.GetFloat("player.aerial.attack.boost");
		}

		public float JumpSpeed { get; }
		public float SlideThreshold { get; }
		public float LowerWallThreshold { get; }
		public float UpperWallThreshold { get; }
		public float AerialAttackBoost { get; }
	}
}
