using Zeldo.Entities.Core;

namespace Zeldo.Control
{
	public class SurfaceController : AbstractController
	{
		public SurfaceController(Actor parent) : base(parent)
		{
		}

		public override void Update(float dt)
		{
			/*
			var body = Parent.GroundBody;
			var velocity = body.Velocity;

			vec2 direction = Parent.RunDirection;

			// Accelerate.
			if (direction != vec2.Zero)
			{
				velocity += direction * Parent.RunAcceleration * dt;

				float max = Parent.RunMaxSpeed;

				if (Utilities.LengthSquared(velocity) > max * max)
				{
					velocity = Utilities.Normalize(velocity) * max;
				}
			}
			// Decelerate.
			else
			{
				int sign = Math.Sign(velocity.x != 0 ? velocity.x : velocity.y);

				velocity -= Utilities.Normalize(velocity) * Parent.RunDeceleration * dt;

				if (Math.Sign(velocity.x != 0 ? velocity.x : velocity.y) != sign)
				{
					velocity = vec2.Zero;
				}
			}

			body.Velocity = velocity;
			*/
		}
	}
}
