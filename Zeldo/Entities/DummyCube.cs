using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Entities
{
	public class DummyCube : Entity
	{
		private RigidBodyTypes bodyType;

		public DummyCube(RigidBodyTypes bodyType) : base(EntityGroups.Object)
		{
			this.bodyType = bodyType;
		}

		public override void Initialize(Scene scene, JToken data)
		{
			CreateModel(scene, "Cube.obj");

			var body = CreateRigidBody(scene, new BoxShape(new JVector(1)), bodyType);
			body.AffectedByGravity = false;
		}
	}
}
