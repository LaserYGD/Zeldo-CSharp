using System;
using Engine.Physics;
using Engine.Utility;
using GlmSharp;
using Jitter.LinearMath;
using Zeldo.Entities;
using Zeldo.Entities.Player;
using Zeldo.UI;

namespace Zeldo.Control
{
	public class LadderController : AbstractController
	{
		private vec3 velocity;

		// Ladders are designed to only work with the player (rather than generic actors). This may change in the
		// future.
		public LadderController(PlayerCharacter player) : base(player)
		{
		}

		public Ladder Ladder { get; set; }

		// TODO: Add sliding down ladders (similar to Dark Souls).
		public float ClimbAcceleration { get; set; }
		public float ClimbDeceleration { get; set; }
		public float ClimbMaxSpeed { get; set; }
		public float ClimbDistance { get; set; }

		// This should be 1, -1, or 0 (representing up, down, or stationary on the ladder).
		public int Direction { get; set; }

		public override void PreStep(float step)
		{
			var v = Parent.ManualVelocity;
			var ladderBody = Ladder.ControllingBody;

			// Accelerate.
			if (Direction != 0)
			{
				v.y += ClimbAcceleration * Direction * step;
				v.y = Utilities.Clamp(v.y, -ClimbMaxSpeed, ClimbMaxSpeed);
			}
			// Decelerate.
			else if (Utilities.LengthSquared(v) > 0)
			{
				int oldSign = Math.Sign(v.y);

				v.y -= ClimbDeceleration * step * oldSign;

				if (oldSign != Math.Sign(v.y))
				{
					v = vec3.Zero;
				}
			}

			// TODO: Apply orientation as well (for rotating ladders).
			// TODO: Some of this feels very similar to the platform controller. Should be put in a common location somehow.
			Parent.ManualVelocity = v;
			Parent.ManualPosition += (Parent.ManualVelocity * step).ToJVector();

			var p = ladderBody.Position + JVector.Transform(Parent.ManualPosition, ladderBody.Orientation);
			var body = Parent.ControllingBody;
			body.SetTransform(p, body.Orientation, step);
		}
	}
}
