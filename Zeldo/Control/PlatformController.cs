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
			// TODO: Project onto the platform while moving.
			var body = Parent.ControllingBody;
			var orientation = Platform.Orientation;

			// TODO: Consider optimizing the orientation transform by marking the platform entity as fixed rotation.
			body.Position = Platform.Position + JVector.Transform(Parent.PlatformPosition, orientation) +
				new JVector(0, Parent.Height / 2, 0);
			Parent.BodyYaw = orientation.ComputeYaw() + Parent.PlatformYaw;
		}
	}
}
