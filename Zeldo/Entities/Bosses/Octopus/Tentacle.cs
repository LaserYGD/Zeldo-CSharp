using System.Collections.Generic;
using Zeldo.Combat;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Bosses.Octopus
{
	// Tentacles are conceptually somewhere in between actors and regular living entities. They behave in some similar
	// ways to actors (such as using AI functions and triggering attacks), but don't require surface control.
	public class Tentacle : LivingEntity
	{
		private static Dictionary<string, AttackData> dataMap;

		static Tentacle()
		{
			dataMap = AttackData.Load("Octopus.json");
		}

		private Dictionary<string, Attack<Tentacle>> attacks;

		public Tentacle(OctopusBoss parent) : base(EntityGroups.Boss)
		{
			Parent = parent;
			attacks = new Dictionary<string, Attack<Tentacle>>();

			// TODO: Allow different tentacles to be initialized with a subset of attacks (e.g. only weapon-based attacks).
			foreach (var pair in dataMap)
			{
				attacks.Add(pair.Key, pair.Value.Activate(this));
			}
		}

		public OctopusBoss Parent { get; }

		public void TriggerAttack(string name)
		{
			Components.Add(attacks[name]).Start();
		}
	}
}
