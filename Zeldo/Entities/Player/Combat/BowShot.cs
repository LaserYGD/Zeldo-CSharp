using Zeldo.Combat;

namespace Zeldo.Entities.Player.Combat
{
	public class BowShot : Attack<PlayerCharacter>
	{
		public BowShot(AttackData data, PlayerCharacter parent) : base(data, parent)
		{
		}

		// The bow can be drawn and held prior to release.
		public bool ShouldRelease { get; set; }

		protected override bool ShouldAdvance(AttackPhases phase)
		{
			return phase != AttackPhases.Prepare || ShouldRelease;
		}

		protected override void OnExecute()
		{
			// TODO: Spawn an arrow with appropriate velocity.
		}
	}
}
