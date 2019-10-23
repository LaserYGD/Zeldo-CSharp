using Engine.Physics;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Entities
{
	public class MovingPlatform : Entity
	{
		private const float Radius = 3;
		private const float Speed = 0.75f;

		private vec3 scale;
		private JVector pivot;

		private float angle;

		public MovingPlatform(vec3 scale, vec3 pivot) : base(EntityGroups.Platform)
		{
			this.scale = scale;
			this.pivot = pivot.ToJVector();
		}

		public override void Initialize(Scene scene, JToken data)
		{
			var body = CreateBody(scene, new BoxShape(scale.ToJVector()), RigidBodyTypes.Static);
			body.PreStep = PreStep;

			var model = CreateModel(scene, "Cube.obj");
			model.Scale = scale;

			base.Initialize(scene, data);
		}

		private void PreStep(float step)
		{
			angle += Speed * step;

			vec2 v = Utilities.Direction(angle) * Radius;

			controllingBody.Position = pivot + new JVector(v.x, 0, v.y);
			controllingBody.Orientation = quat.FromAxisAngle(angle * 2, vec3.UnitY).ToJMatrix();
		}
	}
}
