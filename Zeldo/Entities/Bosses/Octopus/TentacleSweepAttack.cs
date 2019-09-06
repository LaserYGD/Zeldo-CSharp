using Zeldo.Combat;

namespace Zeldo.Entities.Bosses.Octopus
{
	// Unlike the slam and thrust, the sweep attack doesn't hover and track the player. Instead, the tentacle sets down
	// near one of the sides of the arena, then, after a moment, quickly sweeps across. The attack can optionally
	// sweep twice (back and forth). Once the sweep has finished, the tentacle remains vulnerable for a few seconds.
	public class TentacleSweepAttack : AttackData
	{
	}
}
