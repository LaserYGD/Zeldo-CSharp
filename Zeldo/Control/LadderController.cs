using System;
using Engine.Utility;
using GlmSharp;
using Zeldo.Entities;
using Zeldo.Entities.Player;

namespace Zeldo.Control
{
	public class LadderController : AbstractController
	{
		// Ladders use a custom progress value (representing the distance climbed from the bottom).
		private float progress;

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

		public void OnMount(Ladder ladder, PlayerCharacter player)
		{
			Ladder = ladder;
			progress = player.Position.y - ladder.Position.y;
		}

		public override void PreStep(float step)
		{
			var body = Parent.ControllingBody;
			var v = body.LinearVelocity;

			// Accelerate.
			if (Direction != 0)
			{
				v.Y += ClimbAcceleration * Direction * step;
				v.Y = Utilities.Clamp(v.Y, -ClimbMaxSpeed, ClimbMaxSpeed);
			}
			// Decelerate.
			else if (v.Y != 0)
			{
				int oldSign = Math.Sign(v.Y);

				v.Y -= ClimbDeceleration * step * oldSign;

				if (oldSign != Math.Sign(v.Y))
				{
					v.Y = 0;
				}
			}

			body.LinearVelocity = v;

			/*
			var f = Ladder.FacingDirection;

			progress += v.Y * step;
			Parent.Position = Ladder.Position + new vec3(0, progress, 0) + new vec3(f.x, 0, f.y) * ClimbDistance;
			*/
		}
	}
}
