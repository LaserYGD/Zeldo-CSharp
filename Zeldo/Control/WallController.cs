using System;
using Engine;
using Engine.Core;
using Engine.Physics;
using Engine.Timing;
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

		// Normal and surface are mutually exclusive. Surfaces are used for the static world mesh, while the direct
		// normal is used for wall jumping off pseudo-static bodies (like platforms).
		public vec3 Normal { get; set; }
		public SurfaceTriangle Wall { get; set; }

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
			// This means the player is on the static world mesh (rather than a moving platform).
			if (Wall != null)
			{
			}
		}
	}
}
