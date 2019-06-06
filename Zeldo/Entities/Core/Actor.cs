using Engine.Physics;
using Engine.Shapes._2D;
using GlmSharp;
using Zeldo.Physics._2D;

namespace Zeldo.Entities.Core
{
	public abstract class Actor : LivingEntity
	{
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

		public override void Update(float dt)
		{
			Components.Update(dt);
			selfUpdate = true;

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
