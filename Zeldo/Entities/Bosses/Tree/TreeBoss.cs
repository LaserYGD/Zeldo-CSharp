using Zeldo.Entities.Core;

namespace Zeldo.Entities.Bosses.Tree
{
	// TODO: Should this class extend Boss? It doesn't actually use a traditional health bar.
	public class TreeBoss : LivingEntity
	{
		public TreeBoss() : base(EntityGroups.Boss)
		{
		}
	}
}
