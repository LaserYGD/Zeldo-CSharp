using Engine;

namespace Zeldo.Entities
{
	public class PlayerData
	{
		public PlayerData()
		{
			JumpSpeed = Properties.GetFloat("player.jump.speed");
			JumpLimit = Properties.GetFloat("player.jump.limit");
			JumpDeceleration = Properties.GetFloat("player.jump.deceleration");
			AscendAcceleration = Properties.GetFloat("player.ascend.acceleration");
			AscendTargetSpeed = Properties.GetFloat("player.ascend.target.speed");
			SlideThreshold = Properties.GetFloat("player.slide.threshold");
			UpperWallThreshold = Properties.GetFloat("player.upper.wall.threshold");
			LowerWallThreshold = Properties.GetFloat("player.lower.wall.threshold");
			AerialAttackBoost = Properties.GetFloat("player.aerial.attack.boost");
		}

		public float JumpSpeed { get; }
		public float JumpLimit { get; }
		public float JumpDeceleration { get; }
		public float AscendAcceleration { get; }
		public float AscendTargetSpeed { get; }
		public float SlideThreshold { get; }
		public float LowerWallThreshold { get; }
		public float UpperWallThreshold { get; }
		public float AerialAttackBoost { get; }
	}
}
