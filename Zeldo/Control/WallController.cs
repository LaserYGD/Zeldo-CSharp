using System;
using System.Linq;
using Engine.Core;
using Engine.Physics;
using Engine.Utility;
using GlmSharp;
using Zeldo.Entities.Player;
using Zeldo.Physics;

namespace Zeldo.Control
{
	public class WallController : AbstractController
	{
		private float acceleration;
		private float deceleration;
		private float maxSpeed;

		private TimedFlag wallStickFlag;

		// For the time being, only the player is capable of traversing walls.
		public WallController(PlayerCharacter player, TimedFlag wallStickFlag) : base(player)
		{
			this.wallStickFlag = wallStickFlag;
		}

		public vec2 FlatDirection { get; set; }

		// Normal and surface are mutually exclusive. Surfaces are used for the static world mesh, while the direct
		// normal is used for wall jumping off pseudo-static bodies (like platforms).
		public vec3 Normal { get; set; }
		public SurfaceTriangle Wall { get; set; }

		public void Initialize(float acceleration, float deceleration, float maxSpeed)
		{
			this.acceleration = acceleration;
			this.deceleration = deceleration;
			this.maxSpeed = maxSpeed;
		}

		public override void PreStep(float step)
		{
			var body = Parent.ControllingBody;
			var v = body.LinearVelocity.ToVec3();

			// TODO: Apply manual Y velocity (especially when moving down, a bit slower than gravity).
			// TODO: Consider applying wall press logic (i.e. only move side to side if you're angled enough).
			var flatV = v.swizzle.xz;

			// Acceleration
			if (Utilities.LengthSquared(FlatDirection) > 0)
			{
				// TODO: Cap max speed based on flat direction.
				var perpendicular = Wall.FlatNormal.swizzle.xz;
				perpendicular = new vec2(-perpendicular.y, perpendicular.x);
				flatV += Utilities.Project(FlatDirection, perpendicular) * acceleration * step;

				if (Utilities.LengthSquared(flatV) > maxSpeed * maxSpeed)
				{
					flatV = Utilities.Normalize(flatV) * maxSpeed;
				}
			}
			// Deceleration
			else if (Utilities.LengthSquared(flatV) > 0)
			{
				int oldSign = Math.Sign(flatV.x != 0 ? flatV.x : flatV.y);

				flatV -= Utilities.Normalize(flatV) * deceleration * step;

				int newSign = Math.Sign(flatV.x != 0 ? flatV.x : flatV.y);

				if (oldSign != newSign)
				{
					flatV = vec2.Zero;
				}
			}

			v.x = flatV.x;
			v.z = flatV.y;
			body.LinearVelocity = v.ToJVector();

			// TODO: Apply a thin forgiveness range for staying on a wall.
			var d = Utilities.Dot(FlatDirection, Wall.FlatNormal.swizzle.xz);

			// This means that the flat direction is pressing away the wall.
			if (d > 0)
			{
			}

			body.LinearVelocity = v.ToJVector();
		}

		public override void PostStep(float step)
		{
			// This means the player is on the static world mesh (rather than a moving platform).
			if (Wall != null)
			{
			}
		}
	}
}
