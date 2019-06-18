using System;
using Engine.Utility;
using GlmSharp;
using Zeldo.Entities.Core;

namespace Zeldo.Controllers
{
	public static class ControlHelper
	{
		public static vec2 ApplyMovement(vec2 velocity, Actor actor, float dt)
		{
			vec2 direction = actor.RunDirection;

			if (direction != vec2.Zero)
			{
				velocity += direction * actor.RunAcceleration * dt;

				float max = actor.RunMaxSpeed;

				if (Utilities.LengthSquared(velocity) > max * max)
				{
					velocity = Utilities.Normalize(velocity) * max;
				}
			}
			else
			{
				int sign = Math.Sign(velocity.x != 0 ? velocity.x : velocity.y);

				velocity -= Utilities.Normalize(velocity) * actor.RunDeceleration * dt;

				if (Math.Sign(velocity.x != 0 ? velocity.x : velocity.y) != sign)
				{
					velocity = vec2.Zero;
				}
			}

			return velocity;
		}
	}
}
