using Zeldo.Entities.Core;

namespace Zeldo.Controllers
{
	public class RunController : CharacterController
	{
		public RunController(Actor parent) : base(parent)
		{
		}

		public override void Update(float dt)
		{
			var body = Parent.GroundBody;
			body.Velocity = ControlHelper.ApplyMovement(body.Velocity, Parent, dt);
		}
	}
}
