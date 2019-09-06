using Zeldo.Combat;

namespace Zeldo.Entities.Bosses.Octopus
{
	// During a slam attack, the tentacle hovers above the player for a few moments (and tracks movement) before
	// locking position and quickly slamming down. The tentacle then remains vulnerable for a short time before
	// resetting.
	public class TentacleSlamAttack : Attack<Tentacle>
	{
		public TentacleSlamAttack(AttackData data, Tentacle parent) : base(data, parent)
		{
		}
	}
}
