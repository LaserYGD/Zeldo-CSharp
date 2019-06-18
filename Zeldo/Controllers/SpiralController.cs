using Engine.Shapes._2D;
using Engine.Utility;
using GlmSharp;
using Zeldo.Entities.Core;

namespace Zeldo.Controllers
{
	public class SpiralController : CharacterController
	{
		private SpiralStaircase staircase;

		public SpiralController(SpiralStaircase staircase, Actor parent) : base(parent)
		{
			this.staircase = staircase;
		}

		public override void Update(float dt)
		{
			// Although the ground body isn't used in the same way on spiral staircases, some of its fields are still
			// used to control movement (velocity and radius specifically).
			var body = Parent.GroundBody;

			vec2 velocity = body.Velocity;
			vec2 stairPosition = Parent.StairPosition;
			vec3 p = staircase.Position;

			// The staircase position doesn't correspond to an actual world position directly. Instead, X represents
			// the radial distance around the central axis, while Y is the distance from that same axis.
			float angle = stairPosition.x;
			float radius = stairPosition.y;
			float bodyRadius = ((Circle)body.Shape).Radius;

			// Y velocity needs to be applied before X (since radius affects how X velocity is converted to radians).
			stairPosition.y += velocity.y * dt;
			stairPosition.y = Utilities.Clamp(stairPosition.y, staircase.InnerRadius + bodyRadius,
				staircase.OuterRadius - bodyRadius);

			// The actor's X velocity needs to be converted to angular speed. Since arc distance = theta * radius,
			// theta = arc distance (i.e. speed) / radius.
			velocity.x /= radius;
			stairPosition.x += velocity.x * dt * (staircase.IsClockwise ? -1 : 1);

			float y = staircase.Slope * angle;

			vec2 v = Utilities.Direction(angle + staircase.Rotation) * radius;

			body.Position = v + p.swizzle.xz;
			body.Elevation = y + p.y;

			Parent.StairPosition = stairPosition;
		}
	}
}
