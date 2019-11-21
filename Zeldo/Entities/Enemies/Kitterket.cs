using Engine;
using Engine.Utility;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Enemies
{
	/**
	 * Name inspired as a variation of "kitty cat". Rare creature with the ability to attune to various elements.
	 * Generally skittish and tricky to catch, but will fight in a pinch. Drops rare loot. A fixed number of kitterkets
	 * exist across the overworld map.
	 */
	public class Kitterket : Enemy
	{
		private static float startleDistance;

		static Kitterket()
		{
			//startleDistance = Properties.GetFloat("kitterket.startle.distance");
		}

		public override void Initialize(Scene scene, JToken data)
		{
			base.Initialize(scene, data);
		}

		/**
		 * AI logic:
		 *
		 * - When the player is nearby (and in-view), play an alert animation before running away.
		 * - While running, use pathfinding logic that aligns with the skills the player is likely to have in that
		 *   zone (such that the creature doesn't "cheat" and jump somewhere the player can't follow).
		 * - If cornered, turn and begin fighting the player.
		 */
		public override void Update(float dt)
		{
			if (Utilities.DistanceSquared(Position, player.Position) <= startleDistance * startleDistance)
			{
				Startle();
			}
		}

		private void Startle()
		{
		}
	}
}
