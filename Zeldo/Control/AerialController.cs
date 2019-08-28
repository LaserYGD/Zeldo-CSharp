using System;
using Engine.Physics;
using Engine.Utility;
using GlmSharp;
using Jitter.LinearMath;
using Zeldo.Entities.Core;

namespace Zeldo.Control
{
	// This class is specifically meant for aerial entities that are affected by gravity and accelerate on the flat X
	// and Z axes (such as the player or regular ground-based enemies who can jump).
	public class AerialController : AbstractController
	{
		public AerialController(Actor parent) : base(parent)
		{
		}

		public float Acceleration { get; set; }
		public float Deceleration { get; set; }
		public float MaxSpeed { get; set; }

		public vec2 FlatDirection { get; set; }

		public override void Update(float dt)
		{
			var body = Parent.ControllingBody;
			var v = body.LinearVelocity;
			var flat = v.ToVec3().swizzle.xz;

			// Acceleration.
			if (FlatDirection != vec2.Zero)
			{
				flat += FlatDirection * Acceleration * dt;

				// TODO: Consider setting the new direction exactly as the velocity approaches the asymptote.
				if (Utilities.LengthSquared(flat) > MaxSpeed * MaxSpeed)
				{
					flat = Utilities.Normalize(flat) * MaxSpeed;
				}
			}
			// Deceleration.
			else if (flat != vec2.Zero)
			{
				int oldSign = Math.Sign(flat.x != 0 ? flat.x : flat.y);

				flat -= Utilities.Normalize(flat) * Deceleration * dt;

				int newSign = Math.Sign(flat.x != 0 ? flat.x : flat.y);

				if (newSign != oldSign)
				{
					flat = vec2.Zero;
				}
			}
			
			body.LinearVelocity = new JVector(flat.x, v.Y, flat.y);
		}
	}
}
