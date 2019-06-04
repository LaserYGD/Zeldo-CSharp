using Engine;

namespace Zeldo.Entities
{
	public class PlayerData
	{
		public PlayerData()
		{
			RunAcceleration = Properties.GetFloat("player.run.acceleration");
			RunDeceleration = Properties.GetFloat("player.run.deceleration");
			RunMaxSpeed = Properties.GetFloat("player.run.max.speed");
		}

		public float RunAcceleration { get; set; }
		public float RunDeceleration { get; set; }
		public float RunMaxSpeed { get; set; }
	}
}
