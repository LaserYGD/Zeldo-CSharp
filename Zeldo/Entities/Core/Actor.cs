using Engine.Physics;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Zeldo.Control;

namespace Zeldo.Entities.Core
{
	public abstract class Actor : LivingEntity
	{
		private CharacterController controller;

		private float halfHeight;

		protected bool onGround;

		protected Actor(EntityGroups group) : base(group)
		{
		}
		
		// TODO: This could probably be protected rather than public.
		public float Height
		{
			get => halfHeight * 2;
			set => halfHeight = value / 2;
		}

		public float RunAcceleration { get; protected set; }
		public float RunDeceleration { get; protected set; }
		public float RunMaxSpeed { get; protected set; }

		// This is used by external controllers to accelerate the actor in a desired direction.
		public vec2 RunDirection { get; set; }

		public override vec3 Position
		{
			get => base.Position;
			set
			{
				if (onGround)
				{
					// TODO: Is this redundant with the same code in Entity?
					// The controlling body will probably always exist once the actor is finished, but it might not
					// exist during development.
					if (!selfUpdate && controllingBody != null)
					{
						controllingBody.Position = value.ToJVector();
					}
				}

				base.Position = value;
			}
		}

		// A separate velocity is used for controlled movement along a surface (such as the ground). The controlling
		// body's velocity can't be easily reused because it'd constantly be affected by the physics engine.
		public vec3 SurfaceVelocity { get; set; }

		// This is used by external controllers.
		public bool OnGround => onGround;

		protected void CreateKinematicBody(Scene scene, Shape shape)
		{
			var body = CreateRigidBody(scene, shape, RigidBodyTypes.Kinematic);
			body.ShouldIgnore = ShouldIgnore;
		}

		protected virtual bool ShouldIgnore(RigidBody other)
		{
			// TODO: Handle other surfaces as well (i.e. walls rather than just the ground).
			// Actors ignore collisions with the static world mesh while grounded (or otherwise on a surface).
			return onGround && other.Shape is TriangleMeshShape;
		}

		// This function should be used when the actor is on a controlled surface (such as the ground or a wall).
		// While on a surface, the kinematic body is controlled using computed velocity rather than a direct position
		// set.
		// TODO: If delta time isn't needed, this could be merged back into the main Position property.
		public void SetSurfacePosition(vec3 p)
		{
			vec3 midPosition = p + new vec3(0, Height / 2, 0);

			// Using the base version ensures that the body's position isn't set directly.
			// TODO: Consider using an epsilon to determine whether the new position is different from the old one (optimization to avoid recomputing attachments).
			base.Position = midPosition;

			// JVector doesn't have a divide function (which is why conversions happen both ways here).
			// TODO: Is a divide by dt needed here? With that division, velocities felt wrong.
			//controllingBody.LinearVelocity = ((midPosition - controllingBody.Position.ToVec3()) / dt).ToJVector();
			controllingBody.LinearVelocity = (midPosition - controllingBody.Position.ToVec3()).ToJVector();
		}

		public void Attach(CharacterController controller, bool shouldComputeImmediately = false)
		{
			this.controller = controller;

			controller.Parent = this;

			// Calling this function here ensures the actor will be positioned properly the moment it touches a new
			// surface (which commonly causes the controller to change).
			if (shouldComputeImmediately)
			{
				controller.Update(0);
			}
		}

		public void PlayAnimation(string animation)
		{
		}

		public override void Update(float dt)
		{
			Components.Update(dt);
			selfUpdate = true;
			controller?.Update(dt);

			if (onGround)
			{
				//Position = new vec3(p.x, groundBody.Elevation + halfHeight, p.y);
			}
			// It's assumed that all actors capable of going airborne have a controlling 3D body set.
			else
			{
				Position = controllingBody.Position.ToVec3();
				Orientation = controllingBody.Orientation.ToQuat();
			}

			selfUpdate = false;
		}
	}
}
