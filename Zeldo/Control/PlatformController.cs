using System;
using Engine.Physics;
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

			var body = Parent.ControllingBody;
			var orientation = Platform.Orientation;
			var p = Platform.Position + JVector.Transform(Parent.ManualPosition, orientation) +
				new JVector(0, Parent.FullHeight / 2, 0);
			var yaw = orientation.ComputeYaw() + Parent.ManualYaw;

			// TODO: Consider optimizing the orientation transform by marking the platform entity as fixed rotation.
			body.SetTransform(p, JMatrix.CreateFromAxisAngle(JVector.Up, yaw), step);
			Parent.BodyYaw = yaw;
		}

		public override void PostStep(float step)
		{
			// TODO: This can be optimized (probably by having knowledge of the parent shape).
			// TODO: Probably use properties for these hardcoded values (or compute them in some way).
			var n = Platform.Orientation.ToQuat() * vec3.UnitY;
			var v = new vec3(0, Parent.FullHeight / 2 - 0.1f, 0);
			var start = Parent.ControllingBody.Position.ToVec3() - Utilities.Project(v, n);

			// TODO: Consider applying edge forgiveness.
			if (!PhysicsUtilities.Raycast(Parent.Scene.World, Platform, start, -n, 0.5f, out var results))
			{
				Parent.BecomeAirborneFromLedge();
			}
		}
	}
}
