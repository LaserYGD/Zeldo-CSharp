using Engine;
using GlmSharp;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Windmill
{
	public class WindmillBlades : Entity
	{
		private MotorTree motorTree;
		private vec3 axis;

		public WindmillBlades() : base(EntityGroups.Object)
		{
			axis = -vec3.UnitZ;
			IsPersistent = true;
		}

		public override void Initialize(Scene scene, JToken data)
		{
			int blades = data["Blades"].Value<int>();

			// This is the inner radius (of the object to which the blades are attached).
			float radius = data["Radius"].Value<float>();

			string type = data["Type"].Value<string>();

			var mesh = ContentCache.GetMesh("WindmillBlade" + type + ".obj");

			for (int i = 0; i < blades; i++)
			{
				float angle = Constants.TwoPi / blades * i;

				quat orientation = quat.FromAxisAngle(angle, axis);
				vec3 p = vec3.UnitX * radius * orientation;

				CreateModel(scene, mesh, p, orientation);
			}

			base.Initialize(scene, data);
		}

		public override void Update(float dt)
		{
			// TODO: Create and update the motor tree (with the blades as a source).
			//motorTree.Update(dt);

			base.Update(dt);
		}
	}
}
