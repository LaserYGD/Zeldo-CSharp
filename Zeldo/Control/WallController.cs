using System;
using Engine;
using Engine.Physics;
using Engine.Timing;
using Engine.Utility;
using GlmSharp;
using Jitter.Dynamics;
using Zeldo.Entities.Player;
using Zeldo.Physics;

namespace Zeldo.Control
{
	public class WallController : AbstractController
	{
		private vec3 normal;
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

		// Normal and surface are mutually exclusive. Surfaces are used for the static world mesh, while the direct
		// normal is used for wall jumping off pseudo-static bodies (like platforms).
		public void Refresh(RigidBody body, SurfaceTriangle wall)
		{
			this.wall = wall;

			wallBody = body;
		}

		public void Refresh(RigidBody body, vec3 normal)
		{
			this.normal = normal;

			wallBody = body;
		}

		public void Reset()
		{
			wall = null;
			wallBody = null;
		}

		// TODO: Apply edge forgiveness when sliding off the edge of a triangle (and becoming airborne).
		public override void PreStep(float step)
		{
			var body = Parent.ControllingBody;
			var v = body.LinearVelocity.ToVec3();

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
				var perpendicular = Wall.FlatNormal.swizzle.xz;
				perpendicular = new vec2(-perpendicular.y, perpendicular.x);
				flatV += Utilities.Project(FlatDirection, perpendicular) * acceleration * step;

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
			body.LinearVelocity = v.ToJVector();

			// TODO: Apply a thin forgiveness range for staying on a wall.
			var d = Utilities.Dot(FlatDirection, Wall.FlatNormal.swizzle.xz);

			// This means that the flat direction is pressing away the wall. A thin forgiveness angle (specified as a
			// dot product value) is used to help stick the player while still moving in a direction *near* parallel
			// to the wall.
			if (d > stickForgiveness)
			{
				wallStickTimer.IsPaused = false;
			}

			body.LinearVelocity = v.ToJVector();
		}

		public override void PostStep(float step)
		{
			// This raycast distance is arbitrary. Should be short, but also long enough to catch transitions from
			// one wall triangle to another.
			const float RaycastLength = 0.5f;

			// This means the player is on the static world mesh (rather than a moving platform).
			if (wall != null)
			{
				var player = (PlayerCharacter)Parent;
				var radius = player.CapsuleRadius;
				var body = Parent.ControllingBody;

				// This point is used for raycasting. Pulling back the starting position by a small amount should
				// increase stability against potential floating-point inconsistencies near the wall.
				var n = wall.FlatNormal;
				var p = body.Position.ToVec3() - n * (radius - 0.01f);

				if (PhysicsUtilities.Raycast(Parent.Scene.World, wallBody, p, -n, RaycastLength,
					out var results))
				{
					var triangle = results.Triangle;

					// TODO: Verify that the new triangle isn't acute enough to cause a sideways collision instead.
					if (!wall.IsSame(triangle))
					{
						wall = new SurfaceTriangle(triangle, results.Normal, 0, null, true);
					}

					// Note that the wall (and its flat normal) might have changed here (due to the condition above).
					body.Position = (results.Position + wall.FlatNormal * radius).ToJVector();

					return;
				}

				// TODO: Consider adding edge forgiveness.
				// By this point, the player has moved off the current triangle (without transitioning to a new one).
				player.BecomeAirborneFromWall();
			}
		}
	}
}
