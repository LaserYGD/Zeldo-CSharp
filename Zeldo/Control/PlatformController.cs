using Engine.Physics;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Zeldo.Entities.Core;

namespace Zeldo.Control
{
	public class PlatformController : AbstractController
	{
		public PlatformController(Actor parent) : base(parent)
		{
		}

		public RigidBody Platform { get; set; }

		public override void PreStep(float step)
		{
			var body = Parent.ControllingBody;
			var orientation = Platform.Orientation;
			var p = Platform.Position + JVector.Transform(Parent.ManualPosition, orientation) +
				new JVector(0, Parent.Height / 2, 0);
			var yaw = orientation.ComputeYaw() + Parent.ManualYaw;

			// TODO: Consider optimizing the orientation transform by marking the platform entity as fixed rotation.
			body.SetTransform(p, JMatrix.CreateFromAxisAngle(JVector.Up, yaw), step);
			Parent.BodyYaw = yaw;
		}
	}
}
