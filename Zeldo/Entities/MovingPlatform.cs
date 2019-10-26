using System.Collections.Generic;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Physics;
using Engine.Timing;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.UI;

namespace Zeldo.Entities
{
	public class MovingPlatform : Entity, IReceiver
	{
		private const float AngularSpeed = 2.5f;

		private vec3 scale;
		private vec3 p1;
		private vec3 p2;

		private RepeatingTimer timer;

		private float angle;

		private bool direction;

		public MovingPlatform(vec3 scale, vec3 p1, vec3 p2, float duration) : base(EntityGroups.Platform)
		{
			this.scale = scale;
			this.p1 = p1;
			this.p2 = p2;

			if (p1.y == p2.y)
			{
				//this.p2 = p1;
			}

			timer = new RepeatingTimer(progress =>
			{
				direction = !direction;

				return true;
			}, duration);
		}

		public List<MessageHandle> MessageHandles { get; set; }

		public override void Initialize(Scene scene, JToken data)
		{
			var body = CreateBody(scene, new BoxShape(scale.ToJVector()), RigidBodyTypes.PseudoStatic);
			body.PreStep = PreStep;

			var model = CreateModel(scene, "Cube.obj");
			model.Scale = scale;

			base.Initialize(scene, data);
		}

		private void PreStep(float step)
		{
			const float MaxTilt = 1.5f;

			timer.Update(step);

			var t = timer.Progress;

			if (direction)
			{
				t = 1 - t;
			}

			t = Ease.Compute(t, EaseTypes.QuadraticInOut);
			angle += AngularSpeed * step;

			float tilt = Ease.Compute(t, EaseTypes.QuadraticInOut) * MaxTilt - MaxTilt / 2;

			var p = vec3.Lerp(p1, p2, Ease.Compute(t, EaseTypes.Linear)).ToJVector();
			var orientation = p1.y == p2.y
				? quat.FromAxisAngle(angle, vec3.UnitY) * quat.FromAxisAngle(tilt, vec3.UnitX)
				: quat.Identity;

			controllingBody.SetTransform(p, orientation.ToJMatrix(), step);
		}
	}
}
