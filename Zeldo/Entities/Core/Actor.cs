using Engine.Physics;
using Engine.Shapes._2D;
using GlmSharp;
using Zeldo.Physics._2D;

namespace Zeldo.Entities.Core
{
	public abstract class Actor : LivingEntity
	{
		protected bool onGround;

		protected RigidBody2D groundBody;

		protected Actor(EntityGroups group) : base(group)
		{
		}

		protected RigidBody2D CreateGroundBody(Scene scene, float radius)
		{
			groundBody = new RigidBody2D(new Circle(radius));
			groundBody.Position = Position.swizzle.xz;
			groundBody.Elevation = Position.z;

			scene.World2D.Add(groundBody);

			return groundBody;
		}

		public override void Update(float dt)
		{
			Components.Update(dt);
			selfUpdate = true;

			if (onGround)
			{
				var p = groundBody.Position;

				Position = new vec3(p.x, groundBody.Elevation, p.y);
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
