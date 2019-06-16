using System;
using Engine;
using Engine.Shapes._2D;
using Engine.Utility;
using GlmSharp;

namespace Zeldo.Controllers
{
	public class SpiralController : CharacterController
	{
		public SpiralStaircase Staircase { get; set; }

		public override void Update(float dt)
		{
			// Although the ground body isn't used in the same way on spiral staircases, some of its fields are still
			// used to control movement (velocity and radius specifically).
			var body = Parent.GroundBody;

			vec2 velocity = body.Velocity;
			vec2 stairPosition = Parent.StairPosition;

			// The staircase position doesn't correspond to an actual world position directly. Instead, X represents
			// the radial distance around the central axis, while Y is the distance from that same axis.
			float angle = stairPosition.x;
			float radius = stairPosition.y;
			float bodyRadius = ((Circle)body.Shape).Radius;

			// Y velocity needs to be applied before X (since radius affects how X velocity is converted to radians).
			stairPosition.y += velocity.y * dt;
			stairPosition.y = Utilities.Clamp(stairPosition.y, Staircase.InnerRadius + bodyRadius,
				Staircase.OuterRadius - bodyRadius);

			// The actor's X velocity needs to be converted to angular speed. Since arc distance = theta * radius,
			// theta = arc distance (i.e. speed) / radius.
			velocity.x /= radius * (Staircase.IsClockwise ? -1 : 1);
			stairPosition.x += velocity.x * dt;

			float y = Staircase.Position.y + Staircase.Slope * angle;

			vec2 v = Utilities.Direction(angle) * radius;

			body.Position = v;
			body.Elevation = y;

			Parent.StairPosition = stairPosition;
		}
	}
}
