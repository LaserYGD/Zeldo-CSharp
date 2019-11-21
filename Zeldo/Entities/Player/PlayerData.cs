using Engine;
using Engine.Interfaces;
using Engine.Props;
using Engine.Utility;

namespace Zeldo.Entities.Player
{
	public class PlayerData : IReloadable
	{
		public PlayerData()
		{
			Properties.Access(this);
		}

		// Jumps
		public float JumpSpeed { get; private set; }
		public float JumpLimit { get; private set; }
		public float JumpDeceleration { get; private set; }
		public float DoubleJumpSpeed { get; private set; }

		// In practice, the double jump limit will likely always be the same as the regular jump limit, but it's more
		// future-proof to keep them separate. Jump deceleration, in constrast, is assumed the same between single and
		// double jumps.
		public float DoubleJumpLimit { get; private set; }

		// TODO: Consider adding both small and big fall damage (with separate thresholds).
		// Fall damage
		public int FallDamage { get; private set; }
		public float FallDamageThreshold { get; private set; }

		// Walls
		public float WallJumpFlatSpeed { get; private set; }
		public float WallJumpYSpeed { get; private set; }
		public float WallJumpMaxAngle { get; private set; }
		public float WallPressThreshold { get; private set; }

		// Platforms
		public float PlatformJumpSpeed { get; private set; }
		public float PlatformJumpThreshold { get; private set; }

		// Ascend
		public float AscendAcceleration { get; private set; }
		public float AscendTargetSpeed { get; private set; }

		// Vaults
		public float GroundedVaultThreshold { get; private set; }
		public float AerialVaultThreshold { get; private set; }

		// Other
		public float SlideThreshold { get; private set; }
		public float IdleTime { get; private set; }

		public int KillPlane { get; private set; }

		public void Reload(PropertyAccessor accessor)
		{
			// Jumps
			JumpSpeed = accessor.GetFloat("player.jump.speed");
			JumpLimit = accessor.GetFloat("player.jump.limit");
			JumpDeceleration = accessor.GetFloat("player.jump.deceleration");
			DoubleJumpSpeed = accessor.GetFloat("player.double.jump.speed");
			DoubleJumpLimit = accessor.GetFloat("player.double.jump.limit");

			// Fall damage
			FallDamage = accessor.GetInt("player.fall.damage");
			FallDamageThreshold = accessor.GetFloat("player.fall.damage.threshold");

			// Walls
			var wallJumpSpeed = accessor.GetFloat("player.wall.jump.speed");
			var wallJumpAngle = accessor.GetFloat("player.wall.jump.angle");
			var d = Utilities.Direction(wallJumpAngle);

			// Wall jump values are specified as speed + angle, but stored more simply as flat speed and Y speed.
			WallJumpFlatSpeed = d.x * wallJumpSpeed;
			WallJumpYSpeed = d.y * wallJumpSpeed;
			WallJumpMaxAngle = accessor.GetFloat("player.wall.jump.max.angle");
			WallPressThreshold = accessor.GetFloat("player.wall.press.threshold");

			// Platforms
			PlatformJumpSpeed = accessor.GetFloat("player.platform.jump.speed");
			PlatformJumpThreshold = accessor.GetFloat("player.platform.jump.threshold");

			// Ascend
			AscendAcceleration = accessor.GetFloat("player.ascend.acceleration");
			AscendTargetSpeed = accessor.GetFloat("player.ascend.target.speed");

			// Vaults
			GroundedVaultThreshold = accessor.GetFloat("player.grounded.vault.threshold");
			AerialVaultThreshold = accessor.GetFloat("player.aerial.vault.threshold");

			// Other
			SlideThreshold = accessor.GetFloat("player.slide.threshold");
			IdleTime = accessor.GetFloat("player.idle.time");
			KillPlane = accessor.GetInt("kill.plane");
		}

		public void Dispose()
		{
		}
	}
}
