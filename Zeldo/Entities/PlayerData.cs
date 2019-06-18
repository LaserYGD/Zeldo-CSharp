using Engine;

namespace Zeldo.Entities
{
	public class PlayerData
	{
		public PlayerData()
		{
			JumpSpeed = Properties.GetFloat("player.jump.speed");
		}

		public float JumpSpeed { get; }
	}
}
