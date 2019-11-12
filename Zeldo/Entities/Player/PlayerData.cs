using Engine;
using Engine.Utility;

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

			// Fall damage
			FallDamage = Properties.GetInt("player.fall.damage");
			FallDamageThreshold = Properties.GetFloat("player.fall.damage.threshold");

			// Walls
			var wallJumpSpeed = Properties.GetFloat("player.wall.jump.speed");
			var wallJumpAngle = Properties.GetFloat("player.wall.jump.angle");
			var d = Utilities.Direction(wallJumpAngle);

			// Wall jump values are specified as speed + angle, but stored more simply as flat speed and Y speed.
			WallJumpFlatSpeed = d.x * wallJumpSpeed;
			WallJumpYSpeed = d.y * wallJumpSpeed;
			WallJumpMaxAngle = Properties.GetFloat("player.wall.jump.max.angle");
			WallPressThreshold = Properties.GetFloat("player.wall.press.threshold");

			// Platforms
			PlatformJumpSpeed = Properties.GetFloat("player.platform.jump.speed");
			PlatformJumpThreshold = Properties.GetFloat("player.platform.jump.threshold");

			// Ascend
			AscendAcceleration = Properties.GetFloat("player.ascend.acceleration");
			AscendTargetSpeed = Properties.GetFloat("player.ascend.target.speed");

			// Vaults
			GroundedVaultThreshold = Properties.GetFloat("player.grounded.vault.threshold");
			AerialVaultThreshold = Properties.GetFloat("player.aerial.vault.threshold");

			// Other
			SlideThreshold = Properties.GetFloat("player.slide.threshold");
			IdleTime = Properties.GetFloat("player.idle.time");
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

		// TODO: Consider adding both small and big fall damage (with separate thresholds).
		// Fall damage
		public int FallDamage { get; }
		public float FallDamageThreshold { get; }

		// Walls
		public float WallJumpFlatSpeed { get; }
		public float WallJumpYSpeed { get; }
		public float WallJumpMaxAngle { get; }
		public float WallPressThreshold { get; }

		// Platforms
		public float PlatformJumpSpeed { get; }
		public float PlatformJumpThreshold { get; }

		// Ascend
		public float AscendAcceleration { get; }
		public float AscendTargetSpeed { get; }

		// Vaults
		public float GroundedVaultThreshold { get; }
		public float AerialVaultThreshold { get; }

		// Other
		public float SlideThreshold { get; }
		public float IdleTime { get; }

		public int KillPlane { get; }
	}
}
