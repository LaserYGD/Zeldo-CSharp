using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Physics;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Zeldo.Entities.Core;
using Zeldo.Physics;

namespace Zeldo.Control
{
	public class GroundController : AbstractController
	{
		private static readonly float EdgeForgiveness;

		static GroundController()
		{
			EdgeForgiveness = Properties.GetFloat("edge.forgiveness");
		}

		private float acceleration;
		private float deceleration;
		private float maxSpeed;

		public GroundController(Actor parent) : base(parent)
		{
		}

		public vec2 FlatDirection { get; set; }

		public void Initialize(float acceleration, float deceleration, float maxSpeed)
		{
			this.acceleration = acceleration;
			this.deceleration = deceleration;
			this.maxSpeed = maxSpeed;
		}

		public override void PreStep(float step)
		{
			var body = Parent.ControllingBody;
			body.LinearVelocity = AdjustVelocity(step)
				.ToJVector();

			var vectors = new List<vec3>();

			foreach (Arbiter arbiter in body.Arbiters)
			{
				var contacts = arbiter.ContactList;

				// Before the physics step occurs, all static contacts are aggregated together and manually applied
				// (based on surface normal).
				for (int i = contacts.Count - 1; i >= 0; i--)
				{
					var contact = contacts[i];
					var b1 = contact.Body1;
					var b2 = contact.Body2;

					if (!(b1.IsStatic || b2.IsStatic))
					{
						continue;
					}

					var n = contact.Normal.ToVec3();

					if (body == b1)
					{
						n *= -1;
					}

					var v = Utilities.Normalize(Utilities.ProjectOntoPlane(n, Parent.Ground.Normal));
					var angle = Utilities.Angle(n, v);
					var l = contact.Penetration / (float)Math.Cos(angle);

					vectors.Add(v * l);
					contacts.RemoveAt(i);
				}
			}

			if (vectors.Count > 0)
			{
				ResolveGroundedCollisions(vectors, step);
			}
		}

		public vec3 AdjustVelocity(float step)
		{
			var v = Parent.ControllingBody.LinearVelocity.ToVec3();

			// Deceleration.
			if (FlatDirection == vec2.Zero)
			{
				// The actor is already stopped (and not accelerating).
				if (v == vec3.Zero)
				{
					return v;
				}

				// This assumes the player isn't moving exactly vertically (which shouldn't be possible since the
				// player can't run on walls).
				int oldSign = Math.Sign(v.x != 0 ? v.x : v.z);

				v -= Utilities.Normalize(v) * deceleration * step;

				int newSign = Math.Sign(v.x != 0 ? v.x : v.z);

				if (oldSign != newSign)
				{
					v = vec3.Zero;
				}

				return v;
			}

			// Acceleration.
			var ground = Parent.Ground;

			vec3 normal = ground.Normal;
			vec3 sloped;

			// This means the ground is completely flat (meaning that the flat direction can be used for movement
			// directly).
			if (normal.y == 1)
			{
				sloped = new vec3(FlatDirection.x, 0, FlatDirection.y);
			}
			else
			{
				vec2 flatNormal = Utilities.Normalize(normal.swizzle.xz);

				float d = Utilities.Dot(FlatDirection, flatNormal);
				float y = -ground.Slope * d;

				sloped = Utilities.Normalize(new vec3(FlatDirection.x, y, FlatDirection.y));
			}

			v += sloped * acceleration * step;

			// TODO: Consider correcting the asymptote.
			if (Utilities.LengthSquared(v) > maxSpeed * maxSpeed)
			{
				v = Utilities.Normalize(v) * maxSpeed;
			}

			return v;
		}

		private void ResolveGroundedCollisions(List<vec3> vectors, float step)
		{
			var final = vec3.Zero;

			for (int i = 0; i < vectors.Count; i++)
			{
				var v = vectors[i];
				final += v;

				for (int j = i + 1; j < vectors.Count; j++)
				{
					vectors[j] -= Utilities.Project(v, vectors[j]);
				}
			}

			var jFinal = final.ToJVector();

			// TODO: Is this length check required?
			if (Utilities.LengthSquared(final) > 0.001f)
			{
				var body = Parent.ControllingBody;
				var v = body.LinearVelocity;

				body.Position += v * step + jFinal;
				body.LinearVelocity -= Utilities.Project(v.ToVec3(), final).ToJVector();
			}
		}

		public override void PostStep(float step)
		{
			var body = Parent.ControllingBody;
			var halfHeight = new vec3(0, Parent.FullHeight / 2, 0);
			var p = body.Position.ToVec3() - halfHeight;
			var ground = Parent.Ground;

			// If the projection returns true, that means the actor is still within the current triangle.
			if (ground.Project(p, out vec3 result))
			{
				body.Position = (result + halfHeight).ToJVector();

				return;
			}

			// TODO: Store a better reference to static meshes (rather than querying the world every step).
			var world = Parent.Scene.World;
			var map = world.RigidBodies.First(b => b.Shape is TriangleMeshShape);
			var normal = ground.Normal;
			
			// TODO: Use properties or a constant for the raycast length (and offset).
			// The raycast needs to be offset enough to catch steps.
			if (PhysicsUtilities.Raycast(world, map, p + normal, -normal, 1.2f, out var results) &&
				results.Triangle != null)
			{
				// This means the actor moved to another triangle.
				var newGround = new SurfaceTriangle(results.Triangle, results.Normal, 0);

				// TODO: Is this projection needed? (since the position is already known from the raycast)
				newGround.Project(results.Position, out result);
				body.Position = (result + halfHeight).ToJVector();

				Parent.OnGroundTransition(newGround);

				return;
			}

			// If the actor has moved past a surface triangle (without transitioning to another one), a very small
			// forgiveness distance is checked before signalling the actor to become airborne. This distance is small
			// enough to not be noticeable during gameplay, but protects against potential floating-point errors near
			// the seams of triangles.
			if (SurfaceTriangle.ComputeForgiveness(p, ground) > EdgeForgiveness)
			{
				Parent.BecomeAirborneFromLedge();
			}
		}
	}
}
