using Engine.Physics;
using Jitter.LinearMath;
using Zeldo.Entities.Core;

namespace Zeldo.Controllers
{
	public class AirController : CharacterController
	{
		public AirController(Actor parent) : base(parent)
		{
		}

		public override void Update(float dt)
		{
			var body = Parent.ControllingBody;
			var velocity = body.LinearVelocity;
			var v = ControlHelper.ApplyMovement(velocity.ToVec3().swizzle.xz, Parent, dt);
			
			body.LinearVelocity = new JVector(v.x, velocity.Y, v.y);
		}
	}
}
