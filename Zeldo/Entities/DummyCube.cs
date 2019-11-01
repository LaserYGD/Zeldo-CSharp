using System.Linq;
using Engine.Physics;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.UI;

namespace Zeldo.Entities
{
	public class DummyCube : Entity
	{
		private RigidBodyTypes bodyType;
		private bool isAffectedByGravity;
		private vec3 scale;

		public DummyCube(RigidBodyTypes bodyType, bool isAffectedByGravity, vec3 scale) : base(EntityGroups.Object)
		{
			this.bodyType = bodyType;
			this.isAffectedByGravity = isAffectedByGravity;
			this.scale = scale;
		}

		public RigidBody Body { get; private set; }

		public override void Initialize(Scene scene, JToken data)
		{
			var model = CreateModel(scene, "Cube.obj");
			model.Scale = scale;

			Body = CreateBody(scene, new BoxShape(scale.ToJVector()), bodyType);
			Body.IsAffectedByGravity = isAffectedByGravity;

			base.Initialize(scene, data);
		}

		public override void Update(float dt)
		{
			var list = Scene.Canvas.GetElement<DebugView>().GetGroup("Cube");
			list.Add(controllingBody.Arbiters.Sum(a => a.ContactList.Count).ToString());

			base.Update(dt);
		}
	}
}
