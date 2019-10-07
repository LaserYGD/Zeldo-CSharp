using Engine.Physics;
using Engine.Utility;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Physics;

namespace Zeldo.Entities
{
	public class DummyCapsule : Entity
	{
		private RigidBodyTypes bodyType;

		public DummyCapsule(RigidBodyTypes bodyType) : base(EntityGroups.Object)
		{
			this.bodyType = bodyType;
		}

		public override void Initialize(Scene scene, JToken data)
		{
			CreateModel(scene, "Capsule.obj");

			var body = CreateBody(scene, new CapsuleShape(1, 0.5f), bodyType);
			body.ShouldCollideWith = ShouldCollideWith;

			base.Initialize(scene, data);
		}

		private bool ShouldCollideWith(RigidBody body, JVector[] triangle)
		{
			if (triangle == null)
			{
				return true;
			}

			if (SurfaceTriangle.ComputeSurfaceType(triangle, WindingTypes.CounterClockwise) == SurfaceTypes.Floor)
			{
				return false;
			}

			var n = Utilities.ComputeNormal(triangle[0], triangle[1], triangle[2], WindingTypes.CounterClockwise,
				false);

			return JVector.Dot(controllingBody.LinearVelocity, n) < 0;
		}
	}
}
