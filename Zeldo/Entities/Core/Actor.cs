using System;
using System.Collections.Generic;
using Engine;
using Engine.Physics;
using Engine.Utility;
using GlmSharp;
using Zeldo.Controllers;
using Zeldo.Physics._2D;

namespace Zeldo.Entities.Core
{
	public abstract class Actor : LivingEntity
	{
		private CharacterController controller;

		private float halfHeight;

		protected bool onGround;

		protected RigidBody2D groundBody;

		protected Actor(EntityGroups group) : base(group)
		{
		}

		protected float Height
		{
			get => halfHeight * 2;
			set => halfHeight = value / 2;
		}

		public float RunAcceleration { get; protected set; }
		public float RunDeceleration { get; protected set; }
		public float RunMaxSpeed { get; protected set; }

		// This value is used to precisely control movement up and down spiral staircases (and maybe normal stairs
		// too). X represents progression up the staircase, while Y moves you forward and back within the staircases's
		// inner and outer radii.
		public vec2 StairPosition { get; set; }

		// This is used by external controller to accelerate the actor in a desired direction.
		public vec2 RunDirection { get; protected set; }

		public RigidBody2D GroundBody => groundBody;

		public override vec3 Position
		{
			get => base.Position;
			set
			{
				if (onGround)
				{
					if (!selfUpdate)
					{
						groundBody.Position = value.swizzle.xz;
						groundBody.Elevation = value.y;
					}

					controllingBody3D.Position = value.ToJVector();
				}

				base.Position = value;
			}
		}

		public override void Dispose()
		{
			Scene.World2D.Remove(groundBody);

			base.Dispose();
		}

		public void Attach(CharacterController controller, bool shouldComputeImmediately = false)
		{
			this.controller = controller;

			// Calling this function here ensures the actor will be positioned properly the moment it touches a new
			// surface (which commonly causes the controller to change).
			if (shouldComputeImmediately)
			{
				controller.Update(0);
			}
		}

		public override void OnCollision(Entity entity, vec3 point, vec3 normal)
		{
			if (onGround)
			{
				return;
			}

			if (entity == null && normal.y == 1)
			{
				OnLand();
			}
		}

		protected virtual void OnLand()
		{
			onGround = true;
			groundBody.Elevation = 0;
			groundBody.Velocity = controllingBody3D.LinearVelocity.ToVec3().swizzle.xz;

			Attach(new RunController(this));
		}

		public override void Update(float dt)
		{
			Components.Update(dt);
			selfUpdate = true;
			controller?.Update(dt);

			if (onGround)
			{
				var p = groundBody.Position;
				
				Position = new vec3(p.x, groundBody.Elevation + halfHeight, p.y);
			}
			// It's assumed that all actors capable of going airborne have a controlling 3D body set.
			else
			{
				Position = controllingBody3D.Position.ToVec3();
				Orientation = controllingBody3D.Orientation.ToQuat();
			}

			selfUpdate = false;
		}
	}
}
