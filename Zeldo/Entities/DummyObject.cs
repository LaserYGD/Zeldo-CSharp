using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Entities
{
	public class DummyObject : Entity
	{
		private RigidBodyTypes bodyType;
		private bool affectedByGravity;

		public DummyObject(RigidBodyTypes bodyType, bool affectedByGravity) : base(EntityGroups.Object)
		{
			this.bodyType = bodyType;
			this.affectedByGravity = affectedByGravity;
		}

		public RigidBody Body { get; private set; }

		public override void Initialize(Scene scene, JToken data)
		{
			CreateModel(scene, "Cube.obj");

			Body = CreateBody(scene, new BoxShape(new JVector(1)), bodyType);
			Body.IsAffectedByGravity = affectedByGravity;
		}

	}
}
