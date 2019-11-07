using System;
using Engine.Physics;
using Engine.Shapes._2D;
using Engine.Utility;
using GlmSharp;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Zeldo.Entities.Core;

namespace Zeldo.Control
{
	public class PlatformController : AbstractController
	{
		public PlatformController(Actor parent) : base(parent)
		{
		}

		public RigidBody Platform { get; set; }
		public vec2 FlatDirection { get; set; }

		public override void PreStep(float step)
		{
			const int Acceleration = 60;
			const int Deceleration = 50;
			const int MaxSpeed = 6;

			// TODO: This is similar to aerial and ground (flat) movement. Should be put in a common place.
			var v = Parent.ManualVelocity;
			var flatV = Parent.ManualVelocity.swizzle.xz;

			// Acceleration.
			if (Utilities.LengthSquared(FlatDirection) > 0)
			{
				// TODO: Could have platforms store yaw as well (to avoid the computation).
				var rotated = Utilities.Rotate(FlatDirection, Platform.Orientation.ComputeYaw());
				flatV += rotated * Acceleration * step;

				if (Utilities.LengthSquared(flatV) > MaxSpeed * MaxSpeed)
				{
					flatV = Utilities.Normalize(flatV) * MaxSpeed;
				}
			}
			// Deceleration
			else if (Utilities.LengthSquared(flatV) > 0)
			{
				int oldSign = Math.Sign(flatV.x != 0 ? flatV.x : flatV.y);

				flatV -= Utilities.Normalize(flatV) * Deceleration * step;

				int newSign = Math.Sign(flatV.x != 0 ? flatV.x : flatV.y);

				if (oldSign != newSign)
				{
					flatV = vec2.Zero;
				}
			}

			v.x = flatV.x;
			v.z = flatV.y;

			Parent.ManualVelocity = v;
			Parent.ManualPosition += Parent.ManualVelocity.ToJVector() * step;
		}

		public override void PostStep(float step)
		{
			// TODO: This assumes that the platform will be flat enough that only the top portion (with a known shape) is a floor. Might need ray tracing as an alternative.
			// All platforms are assumed flat (on top, at least). The shape is attached to the rigid body's shape.
			var shape = (Shape2D)Platform.Shape.Tag;

			// TODO; Could make shape optional (for flat platforms), then use ray tracing as an alternative.
			if (!shape.Contains(Parent.ManualPosition.ToVec3().swizzle.xz))
			{
				Parent.BecomeAirborneFromLedge();
			}
		}
	}
}
