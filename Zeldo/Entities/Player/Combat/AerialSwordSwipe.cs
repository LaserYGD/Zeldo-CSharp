using Engine;
using Zeldo.Combat;

namespace Zeldo.Entities.Player.Combat
{
	public class AerialSwordSwipe : Attack<PlayerCharacter>
	{
		private float boost;
		private float limit;

		public AerialSwordSwipe(AttackData data, PlayerCharacter parent) : base(data, parent)
		{
			boost = Properties.GetFloat("player.aerial.swipe.boost");
			limit = Properties.GetFloat("player.aerial.swipe.limit");
		}

		public override bool ShouldTrigger()
		{
			// TODO: Does the jump button need to be held to trigger a swipe? Otherwise, the rapidly-decreasing velocity might look weird with the boost (and swipe animation).
			// Upward swipes must be performed very quickly after starting a jump. As such, the limit here represents
			// the minimum vertical speed required to trigger a swipe (rather than a lift).
			return Parent.ControllingBody.LinearVelocity.Y >= limit;
		}

		protected override void OnExecute()
		{
			var body = Parent.ControllingBody;
			var v = body.LinearVelocity;		
			v.Y += boost;
			body.LinearVelocity = v;
		}
	}
}
