using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Entities.Player;

namespace Zeldo.Entities.Enemies
{
	public abstract class Enemy : Actor
	{
		// Since this game is built to be singleplayer-only, a single, static reference to the player can be used by
		// all enemies.
		protected static PlayerCharacter player;

		// This function should be called once when the gameplay loop is loaded. Using a static scene event doesn't
		// really work since the player needs to be added first.
		public static void AcquirePlayer(Scene scene)
		{
			player = scene.GetEntities<PlayerCharacter>(EntityGroups.Player)[0];
		}

		// Similar to the function above, this function should be called once when the gameplay loop is unloaded.
		// Leaving the player reference intact (even when the player is unloaded) could cause problems with garbage
		// collection.
		public static void InvalidatePlayer()
		{
			player = null;
		}

		protected Enemy() : base(EntityGroups.Enemy)
		{
		}
	}
}
