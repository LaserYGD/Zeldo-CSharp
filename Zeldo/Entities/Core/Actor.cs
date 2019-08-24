using Engine.Physics;
using GlmSharp;
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

		protected float Height
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
