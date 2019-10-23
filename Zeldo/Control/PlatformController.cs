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
			// TODO: Consider optimizing the orientation transform by making the platform entity as fixed rotation.
			Parent.ControllingBody.Position = Platform.Position + JVector.Transform(Parent.PlatformPosition,
				Platform.Orientation);
		}
	}
}
