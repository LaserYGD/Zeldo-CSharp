using System.Collections.Generic;
using System.Diagnostics;
using Engine;
using GlmSharp;
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

			Debug.Assert(blades > 0, "Must have at least one windmill blade.");

			// This is the inner radius (of the object to which the blades are attached).
			var radius = data["Radius"].Value<float>();
			var mesh = ContentCache.GetMesh(data["Mesh"].Value<string>());

			for (int i = 0; i < blades; i++)
			{
				/*
				var angle = Constants.TwoPi / blades * i;
				var orientation = quat.FromAxisAngle(angle, axis);
				var p = vec3.UnitX * radius * orientation;

				CreateModel(scene, mesh, true, p, orientation);
				*/
			}

			// TODO: Apply radius if needed.
			root = new MotorNode(this, 0);
			motorTree = new MotorTree(root);

			base.Initialize(scene, data);
		}

		protected override void ResolveHandles(Scene scene, List<EntityHandle> handles)
		{
		}
	}
}
