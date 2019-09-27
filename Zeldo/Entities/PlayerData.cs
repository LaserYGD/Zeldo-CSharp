using Engine;

namespace Zeldo.Entities
{
	public class PlayerData
	{
		public PlayerData()
		{
			// Jumping
			JumpSpeed = Properties.GetFloat("player.jump.speed");
			JumpLimit = Properties.GetFloat("player.jump.limit");
			JumpDeceleration = Properties.GetFloat("player.jump.deceleration");
			EdgeForgiveness = Properties.GetFloat("player.edge.forgiveness");
			CoyoteJumpTime = Properties.GetFloat("player.coyote.time");

			AscendAcceleration = Properties.GetFloat("player.ascend.acceleration");
			AscendTargetSpeed = Properties.GetFloat("player.ascend.target.speed");
			SlideThreshold = Properties.GetFloat("player.slide.threshold");
			AerialAttackBoost = Properties.GetFloat("player.aerial.attack.boost");

			// Walls (plus steps and vaults)
			WallLowerThreshold = Properties.GetFloat("player.wall.lower.threshold");
			WallUpperThreshold = Properties.GetFloat("player.wall.upper.threshold");
			WallPressThreshold = Properties.GetFloat("player.wall.press.threshold");
			StepThreshold = Properties.GetFloat("player.step.threshold");
			GroundedVaultThreshold = Properties.GetFloat("player.grounded.vault.threshold");
			AerialVaultThreshold = Properties.GetFloat("player.aerial.vault.threshold");
		}

		// Jumping
		public float JumpSpeed { get; }
		public float JumpLimit { get; }
		public float JumpDeceleration { get; }
		public float EdgeForgiveness { get; }
		public float CoyoteJumpTime { get; }

		public float AscendAcceleration { get; }
		public float AscendTargetSpeed { get; }
		public float SlideThreshold { get; }
		public float AerialAttackBoost { get; }

		// Walls (plus steps and vaults)
		public float WallUpperThreshold { get; }
		public float WallLowerThreshold { get; }
		public float WallPressThreshold { get; }
		public float StepThreshold { get; }
		public float GroundedVaultThreshold { get; }
		public float AerialVaultThreshold { get; }
	}
}
