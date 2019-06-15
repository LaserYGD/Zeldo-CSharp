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
			vec3 axis = Staircase.Position;

			// Although the ground body isn't used in the same way on spiral staircases, some of its fields are still
			// used to control movement (velocity and radius specifically).
			var body = Parent.GroundBody;

			float bodyRadius = ((Circle)body.Shape).Radius;

			vec2 velocity = body.Velocity;
			vec2 stairPosition = Parent.StairPosition;
			stairPosition += velocity * dt;

			// The staircase position doesn't correspond to an actual world position directly. Instead, X represents
			// progression up the staicase (between 0 and 1), while Y moves the actor forward and back (within the
			// inner and outer radii).

			// X between 0 and 1
			// Need base staircase lower position (just the stair's position), and top position
			// Need to know total height in order to compute player height
			// From that, can use slope to determine orientation
			float y = Staircase.Position.y + Staircase.Height * stairPosition.x;
			float angle = Staircase.Slope * y;
			float radius = (Staircase.OuterRadius - Staircase.InnerRadius) * stairPosition.y + Staircase.InnerRadius;

			vec2 v = Utilities.Direction(angle) * radius;

			body.Position = v;
			body.Elevation = y;

			Parent.StairPosition = stairPosition;
		}
	}
}
