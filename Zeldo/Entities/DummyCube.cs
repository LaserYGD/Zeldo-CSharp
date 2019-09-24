using System;
using GlmSharp;
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
		private bool affectedByGravity;
		private int hitCount;

		public DummyCube(RigidBodyTypes bodyType, bool affectedByGravity) : base(EntityGroups.Object)
		{
			this.bodyType = bodyType;
			this.affectedByGravity = affectedByGravity;
		}

		public RigidBody Body { get; private set; }

		public override void Initialize(Scene scene, JToken data)
		{
			CreateModel(scene, "Cube.obj");

			Body = CreateRigidBody(scene, new BoxShape(new JVector(1)), bodyType);
			Body.AffectedByGravity = affectedByGravity;
		}

		public override void OnCollision(vec3 p, vec3 normal, vec3[] triangle, float penetration)
		{
			Console.WriteLine($"Hit! ({hitCount++})");
		}
	}
}
