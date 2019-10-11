using Engine;
using Zeldo.Combat;

namespace Zeldo.Entities.Player.Combat
{
	public class AerialSwordLift : Attack<PlayerCharacter>
	{
		private float boost;

		public AerialSwordLift(AttackData data, PlayerCharacter parent) : base(data, parent)
		{
			boost = Properties.GetFloat("player.aerial.sword.boost");
		}

		protected override void OnExecute()
		{
			var body = Parent.ControllingBody;
			var v = body.LinearVelocity;

			// The upward boost is only applied if the player's Y speed is less than the boost. This means that, if
			// triggered during a jump (while still moving up pretty quickly), only a basic swing is performed. If
			// falling (or near the top of a jump), all downward speed is immediately canceled and replaced with a
			// weak lift.
			if (v.Y < boost)
			{
				v.Y = boost;
			}
		}
	}
}
