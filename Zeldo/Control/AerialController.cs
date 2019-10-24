using System;
using Engine.Physics;
using Engine.Utility;
using GlmSharp;
using Jitter.LinearMath;
using Zeldo.Entities.Core;

namespace Zeldo.Control
{
	// This class is built for aerial entities that are affected by gravity and accelerate on the flat X and Z axes
	// (such as the player or ground-based enemies that can jump).
	public class AerialController : AbstractController
	{
		private float acceleration;
		private float deceleration;
		private float maxSpeed;

		private vec2 flatDirection;

		public AerialController(Actor parent) : base(parent)
		{
		}

		public vec2 FlatDirection
		{
			get => flatDirection;
			set
			{
				flatDirection = value;

				// TODO: Improve this (should be able to continue pressing in the direction of movement without losing momentum).
				if (Utilities.LengthSquared(value) > 0)
				{
					IgnoreDeceleration = false;
				}
			}
		}

		// TODO: Cancel this boolean when appropriate.
		// This allows actors (like the player) to maintain momentum when jumping off moving objects (like platforms).
		public bool IgnoreDeceleration { get; set; }

		public void Initialize(float acceleration, float deceleration, float maxSpeed)
		{
			this.acceleration = acceleration;
			this.deceleration = deceleration;
			this.maxSpeed = maxSpeed;
		}

		public override void PreStep(float step)
		{
			var body = Parent.ControllingBody;
			var v = body.LinearVelocity;
			var flat = v.ToVec3().swizzle.xz;

			// Acceleration. This code (plus the deceleration code below) is very similar to how velocity is adjusted
			// for ground movement, but restricted to the flat XZ plane (Y is influenced by gravity instead).
			if (flatDirection != vec2.Zero)
			{
				flat += flatDirection * acceleration * step;

				float max = maxSpeed;

				// TODO: Consider setting the new direction exactly as the velocity approaches the asymptote.
				if (Utilities.LengthSquared(flat) > max * max)
				{
					flat = Utilities.Normalize(flat) * max;
				}
			}
			// Deceleration.
			else if (flat != vec2.Zero && !IgnoreDeceleration)
			{
				int oldSign = Math.Sign(flat.x != 0 ? flat.x : flat.y);

				flat -= Utilities.Normalize(flat) * deceleration * step;

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
