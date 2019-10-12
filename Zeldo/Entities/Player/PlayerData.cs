using Engine;

namespace Zeldo.Entities.Player
{
	public class PlayerData
	{
		public PlayerData()
		{
			// Jumps
			JumpSpeed = Properties.GetFloat("player.jump.speed");
			JumpLimit = Properties.GetFloat("player.jump.limit");
			JumpDeceleration = Properties.GetFloat("player.jump.deceleration");
			DoubleJumpSpeed = Properties.GetFloat("player.double.jump.speed");
			DoubleJumpLimit = Properties.GetFloat("player.double.jump.limit");
			EdgeForgiveness = Properties.GetFloat("player.edge.forgiveness");
			CoyoteJumpTime = Properties.GetFloat("player.coyote.time");

			// Ascend
			AscendAcceleration = Properties.GetFloat("player.ascend.acceleration");
			AscendTargetSpeed = Properties.GetFloat("player.ascend.target.speed");

			// Walls (plus steps and vaults)
			WallLowerThreshold = Properties.GetFloat("player.wall.lower.threshold");
			WallUpperThreshold = Properties.GetFloat("player.wall.upper.threshold");
			WallPressThreshold = Properties.GetFloat("player.wall.press.threshold");
			StepThreshold = Properties.GetFloat("player.step.threshold");
			GroundedVaultThreshold = Properties.GetFloat("player.grounded.vault.threshold");
			AerialVaultThreshold = Properties.GetFloat("player.aerial.vault.threshold");

			// Other
			SlideThreshold = Properties.GetFloat("player.slide.threshold");
			KillPlane = Properties.GetInt("kill.plane");
		}

		// Jumps
		public float JumpSpeed { get; }
		public float JumpLimit { get; }
		public float JumpDeceleration { get; }
		public float DoubleJumpSpeed { get; }

		// In practice, the double jump limit will likely always be the same as the regular jump limit, but it's more
		// future-proof to keep them separate. Jump deceleration, in constrast, is assumed the same between single and
		// double jumps.
		public float DoubleJumpLimit { get; }
		public float EdgeForgiveness { get; }
		public float CoyoteJumpTime { get; }

		// Ascend
		public float AscendAcceleration { get; }
		public float AscendTargetSpeed { get; }

		// Walls (plus steps and vaults)
		public float WallUpperThreshold { get; }
		public float WallLowerThreshold { get; }
		public float WallPressThreshold { get; }
		public float StepThreshold { get; }
		public float GroundedVaultThreshold { get; }
		public float AerialVaultThreshold { get; }

		// Other
		public float SlideThreshold { get; }
		public int KillPlane { get; }
	}
}
