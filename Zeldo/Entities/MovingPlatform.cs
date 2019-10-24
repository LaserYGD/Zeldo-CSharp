using System.Collections.Generic;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Physics;
using Engine.Timing;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.UI;

namespace Zeldo.Entities
{
	public class MovingPlatform : Entity, IReceiver
	{
		private const float AngularVelocity = 1.5f;

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

			timer = new RepeatingTimer(progress =>
			{
				//controllingBody.Position = (direction ? p1 : p2).ToJVector();
				direction = !direction;

				return true;
			}, duration);

			/*
			timer.Tick = t =>
			{
				if (direction)
				{
					t = 1 - t;
				}

				controllingBody.Position = vec3.Lerp(p1, p2, Ease.Compute(t, EaseTypes.QuadraticInOut)).ToJVector();
			};
			*/
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
			timer.Update(step);

			var t = timer.Progress;

			if (direction)
			{
				t = 1 - t;
			}

			var p = vec3.Lerp(p1, p2, Ease.Compute(t, EaseTypes.Linear)).ToJVector();
			controllingBody.SetPosition(p, step);

			var v = controllingBody.LinearVelocity;
			var list = Scene.Canvas.GetElement<DebugView>().GetGroup("Platform");
			list.Add($"Velocity: {v.X:F3} {v.Y:F3} {v.Z:F3}");

			angle += AngularVelocity * step;

			controllingBody.Orientation = (
				quat.FromAxisAngle(angle, vec3.UnitY) *
				quat.FromAxisAngle(0.25f, vec3.UnitZ)).ToJMatrix();
		}
	}
}
