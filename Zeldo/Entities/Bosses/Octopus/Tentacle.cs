using System.Collections.Generic;
using Zeldo.Combat;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Bosses.Octopus
{
	// Tentacles are conceptually somewhere in between actors and regular living entities. They behave in some similar
	// ways to actors (such as using AI functions and triggering attacks), but don't require surface control.
	public class Tentacle : LivingEntity
	{
		private static Dictionary<string, AttackData> attacks;

		static Tentacle()
		{
			attacks = AttackData.Load("Octopus.json");
		}

		public Tentacle() : base(EntityGroups.Boss)
		{
		}
	}
}
