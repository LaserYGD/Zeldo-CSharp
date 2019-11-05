using System.Collections.Generic;
using Engine.Physics;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Windmill
{
	public class WindmillBlades : Entity
	{
		private MotorTree motorTree;
		private MotorNode root;

		public WindmillBlades() : base(EntityGroups.Structure)
		{
		}

		public override void Initialize(Scene scene, JToken data)
		{
			int blades = data["Blades"].Value<int>();
			
			// This is the inner radius (of the object to which the blades are attached).
			var radius = data["Radius"].Value<float>();
			var mesh = data["Mesh"].Value<string>();

			// TODO: Create a polygon shape for the blade.
			var scale = new vec3(8, 0.25f, 3);
			var shape = new BoxShape(scale.ToJVector());
			var offset = new vec3(radius + scale.x / 2, 0, 0);

			FanBodies(scene, shape, blades, offset, vec3.UnitY);

			var models = FanModels(scene, mesh, blades, offset, vec3.UnitY);

			foreach (var model in models)
			{
				model.Scale = scale;
			}

			var body = CreateBody(scene, new CylinderShape(0.25f, 2), RigidBodyTypes.PseudoStatic);
			body.PreStep = step =>
			{
				motorTree.Step(step);
			};

			// TODO: Apply radius if needed.
			root = new MotorNode(this, 0);
			motorTree = new MotorTree(root);
			motorTree.AngularVelocity = 1;

			base.Initialize(scene, data);
		}

		protected override void ResolveHandles(Scene scene, List<EntityHandle> handles)
		{
		}
	}
}
