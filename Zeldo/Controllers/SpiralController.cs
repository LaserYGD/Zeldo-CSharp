using Engine.Shapes._2D;
using GlmSharp;

namespace Zeldo.Controllers
{
	public class SpiralController : CharacterController
	{
		public SpiralStaircase Staircase { get; set; }

		public override void Update(float dt)
		{
			vec3 axis = Staircase.Position;

			// Although the ground body isn't used in the same way on spiral staircases, some of its fields (such as
			// velocity and radius) are still used to control movement.
			var body = Parent.GroundBody;

			float radius = ((Circle)body.Shape).Radius;

			vec2 velocity = body.Velocity;
		}
	}
}
