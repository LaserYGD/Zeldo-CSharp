using Zeldo.Entities.Core;

namespace Zeldo.Entities.Enemies
{
	public abstract class Enemy : LivingEntity
	{
		// Since this game is built to be singleplayer-only, a single, static reference to the player can be used by
		// all enemies.
		private static Player player;

		protected static Player GetPlayer(Scene scene)
		{
			return player ?? (player = scene.GetEntities<Player>(EntityGroups.Player)[0]);
		}

		public static void InvalidatePlayer()
		{
			player = null;
		}

		protected Enemy() : base(EntityGroups.Enemy)
		{
		}
	}
}
