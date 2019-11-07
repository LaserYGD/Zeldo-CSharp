using System.Collections.Generic;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Physics;
using Engine.Shapes._2D;
using Engine.Timing;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Entities
{
	// TODO: Actors seem to lag behind platforms by one step (noticeable when the platform is moving fast and rotating). Should be investigated.
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

			timer = new RepeatingTimer(t =>
			{
				direction = !direction;

				return true;
			}, duration, TimerFlags.None);
		}

		public List<MessageHandle> MessageHandles { get; set; }

		public override void Initialize(Scene scene, JToken data)
		{
			// Attaching a 2D shape (representing the walkable surface) simplifies detecting when an actor runs off the
			// platform (plus it's more efficient). This does require that all platforms use a flat shape as their
			// upper surface, but that should be fine.
			var shape = new BoxShape(scale.ToJVector());
			shape.Tag = new Rectangle(scale.x, scale.z);

			var body = CreateBody(scene, shape, RigidBodyTypes.PseudoStatic);
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
			var o = p1.y == p2.y
				? quat.FromAxisAngle(angle, vec3.UnitY) * quat.FromAxisAngle(tilt, vec3.UnitX)
				: quat.Identity;

			controllingBody.SetTransform(p, o.ToJMatrix(), step);
		}
	}
}
