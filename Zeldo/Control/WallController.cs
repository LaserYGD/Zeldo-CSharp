using System;
using Engine;
using Engine.Physics;
using Engine.Timing;
using Engine.Utility;
using GlmSharp;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Zeldo.Entities.Player;
using Zeldo.Physics;

namespace Zeldo.Control
{
	public class WallController : AbstractController
	{
		// TODO: This will need to be updated too for rotating pseudo-static bodies.
		private vec3 normal;

		// TODO: Should probably recompute this each pre-step (since pseudo-static walls can rotate).
		private vec3 flatNormal;
		private SurfaceTriangle wall;
		private RigidBody wallBody;

		private float acceleration;
		private float deceleration;
		private float maxSpeed;
		private float wallGravity;
		private float wallTerminalSpeed;
		private float stickForgiveness;

		private SingleTimer wallStickTimer;

		// For the time being, only the player is capable of traversing walls.
		public WallController(PlayerCharacter player, SingleTimer wallStickTimer) : base(player)
		{
			this.wallStickTimer = wallStickTimer;

			// Simpler (for the time being, anyway) to just load properties here.
			acceleration = Properties.GetFloat("player.wall.acceleration");
			deceleration = Properties.GetFloat("player.wall.deceleration");
			maxSpeed = Properties.GetFloat("player.wall.max.speed");
			wallGravity = Properties.GetFloat("player.wall.gravity");
			wallTerminalSpeed = Properties.GetFloat("player.wall.terminal.speed");
			stickForgiveness = Properties.GetFloat("player.wall.stick.forgiveness");
		}

		public vec2 FlatDirection { get; set; }

		// This is used by the player to compute the nearest wall point.
		public SurfaceTriangle Wall => wall;
		public RigidBody Body => wallBody;
		public vec3 Normal => normal;

		// Normal and surface are mutually exclusive. Surfaces are used for the static world mesh, while the direct
		// normal is used for wall jumping off pseudo-static bodies (like platforms).
		public void Refresh(RigidBody body, SurfaceTriangle wall)
		{
			this.wall = wall;

			normal = wall.Normal;
			flatNormal = wall.FlatNormal;
			wallBody = body;
		}

		public void Refresh(RigidBody body, vec3 normal, vec3 flatNormal)
		{
			this.normal = normal;
			this.flatNormal = flatNormal;

			wallBody = body;
		}

		public void Reset()
		{
			wall = null;
			wallBody = null;
		}

		// TODO: Apply edge forgiveness when sliding off the edge of a triangle (and becoming airborne).
		// TODO: For pseudo-static wall control, probably need to rotate the normal based on body orientation each step.
		public override void PreStep(float step)
		{
			// The alternative here is body-controlled (i.e. wall control on a pseudo-static body).
			bool isMeshControlled = wall != null;

			var body = Parent.ControllingBody;
			var v = isMeshControlled ? body.LinearVelocity.ToVec3() : Parent.ManualVelocity;

			// "Wall gravity" only applies when moving downward (in order to give the player a little more control when
			// setting up wall jumps).
			v.y -= (v.y > 0 ? PhysicsConstants.Gravity : wallGravity) * step;

			// TODO: Quickly decelerate if the wall is hit at a downward speed faster than terminal.
			if (v.y < -wallTerminalSpeed)
			{
				v.y = -wallTerminalSpeed;
			}

			// TODO: Consider applying wall press logic (i.e. only move side to side if you're angled enough).
			var flatV = v.swizzle.xz;

			// Acceleration
			if (Utilities.LengthSquared(FlatDirection) > 0)
			{
				var perpendicular = flatNormal.swizzle.xz;
				perpendicular = new vec2(-perpendicular.y, perpendicular.x);
				flatV += Utilities.Project(FlatDirection, perpendicular) * acceleration * step;

				// TODO: Quickly decelerate local max if the wall is hit with fast sideways speed.
				// This limits maximum speed based on flat direction. To me, this feels more natural than accelerating
				// up to full speed even when barely moving sideways (relative to the wall).
				var localMax = Math.Abs(Utilities.Dot(FlatDirection, perpendicular)) * maxSpeed;

				if (Utilities.LengthSquared(flatV) > localMax * localMax)
				{
					flatV = Utilities.Normalize(flatV) * localMax;
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

			if (isMeshControlled)
			{
				body.LinearVelocity = v.ToJVector();
			}
			else
			{
				Parent.ManualVelocity = v;
				Parent.ManualPosition += JVector.Transform(v.ToJVector(), JMatrix.Inverse(wallBody.Orientation)) *
					step;

			}

			// TODO: Apply a thin forgiveness range for staying on a wall.
			var d = Utilities.Dot(FlatDirection, flatNormal.swizzle.xz);

			// This means that the flat direction is pressing away the wall. A thin forgiveness angle (specified as a
			// dot product value) is used to help stick the player while still moving in a direction *near* parallel
			// to the wall.
			if (d > stickForgiveness)
			{
				wallStickTimer.IsPaused = false;
			}
		}

		public override void PostStep(float step)
		{
			// This raycast distance is arbitrary. Should be short, but also long enough to catch transitions from
			// one wall triangle to another.
			const float RaycastLength = 0.5f;

			var player = (PlayerCharacter)Parent;
			var radius = player.CapsuleRadius;
			var body = Parent.ControllingBody;

			// The starting point of the raycast is pulled back by a small amount in order to increase stability
			// (without this offset, the player frequently clipped through walls).
			var p = body.Position.ToVec3() - flatNormal * (radius - 0.1f);

			if (PhysicsUtilities.Raycast(Parent.Scene.World, wallBody, p, -flatNormal, RaycastLength,
				out var results))
			{
				// TODO: Might have to re-examine some of this if sliding along a pseudo-static body is allowed (such as the side of a sphere).
				// While sliding along the map mesh, the current triangle can change.
				if (wall != null)
				{
					var triangle = results.Triangle;

					// TODO: Verify that the new triangle isn't acute enough to cause a sideways collision instead.
					if (!wall.IsSame(triangle))
					{
						wall = new SurfaceTriangle(triangle, results.Normal, 0, null, true);
					}
				}

				// Note that for meshes, the triangle (and its flat normal) might have changed here (due to the
				// condition above).
				var result = (results.Position + flatNormal * radius).ToJVector();

				if (wall != null)
				{
					body.Position = (results.Position + flatNormal * radius).ToJVector();
				}
				else
				{
					// TODO: Set orientation as well (for rotation platforms).
					body.SetPosition(result, step);
				}

				return;
			}

			// TODO: Consider adding edge forgiveness.
			// By this point, the player has moved off the current wall (without transitioning to a new triangle).
			player.BecomeAirborneFromWall();
		}
	}
}
