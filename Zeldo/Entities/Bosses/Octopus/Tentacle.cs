using Zeldo.Combat;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Bosses.Octopus
{
	// Tentacles are conceptually somewhere in between actors and regular living entities. They behave in some similar
	// ways to actors (such as using AI functions and triggering attacks), but don't require surface control.
	public class Tentacle : LivingEntity
	{
		// TODO: Allow different tentacles to be initialized with a subset of attacks (e.g. only weapon-based attacks).
		private AttackCollection<Tentacle> attacks;

		public Tentacle(OctopusBoss parent) : base(EntityGroups.Boss)
		{
			Parent = parent;
			attacks = new AttackCollection<Tentacle>("TentacleAttacks.json", this);
		}

		public OctopusBoss Parent { get; }

		public void TriggerAttack(string name)
		{
			Components.Add(attacks[name]).Start();
		}
	}
}
