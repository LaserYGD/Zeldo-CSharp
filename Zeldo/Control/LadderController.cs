using Zeldo.Entities;
using Zeldo.Entities.Grabbable;

namespace Zeldo.Control
{
	public class LadderController : AbstractController
	{
		// Ladders are designed to only work with the player (rather than generic actors). This may change in the
		// future.
		public LadderController(Player player) : base(player)
		{
		}

		public Ladder Ladder { get; set; }

		// TODO: Add sliding down ladders (similar to Dark Souls).
		public float ClimbAcceleration { get; set; }
		public float ClimbDeceleration { get; set; }
		public float ClimbMaxSpeed { get; set; }

		public override void Update(float dt)
		{
			var v = Parent.ControllingBody.LinearVelocity;
		}
	}
}
