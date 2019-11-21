using Engine;
using Engine.Props;
using Zeldo.Combat;

namespace Zeldo.Entities.Player.Combat
{
	public class AerialSwordLift : Attack<PlayerCharacter>
	{
		private float boost;

		public AerialSwordLift(AttackData data, PlayerCharacter parent) : base(data, parent)
		{
			// TODO: Make this reloadable.
			boost = Properties.Access().GetFloat("player.aerial.sword.boost");
		}

		public override bool ShouldTrigger()
		{
			// The upward boost is only applied if the player's Y speed is less than the boost. This means that, if
			// triggered during a jump (while still moving up pretty quickly), only a basic swing is performed. If
			// falling (or near the top of a jump), all downward speed is immediately canceled and replaced with a
			// weak lift.
			return Parent.ControllingBody.LinearVelocity.Y < boost;
		}

		protected override void OnExecute()
		{
			var body = Parent.ControllingBody;
			var v = body.LinearVelocity;
			v.Y = boost;
			body.LinearVelocity = v;
		}
	}
}
