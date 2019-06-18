using System;
using Engine.Utility;
using GlmSharp;
using Zeldo.Entities.Core;

namespace Zeldo.Controllers
{
	public class RunController : CharacterController
	{
		public RunController(Actor parent) : base(parent)
		{
		}

		public override void Update(float dt)
		{
			var body = Parent.GroundBody;

			vec2 velocity = body.Velocity;
			vec2 direction = Parent.RunDirection;
			
			if (direction != vec2.Zero)
			{
				velocity += direction * Parent.RunAcceleration * dt;

				float max = Parent.RunMaxSpeed;

				if (Utilities.LengthSquared(velocity) > max * max)
				{
					velocity = Utilities.Normalize(velocity) * max;
				}
			}
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
		}
	}
}
