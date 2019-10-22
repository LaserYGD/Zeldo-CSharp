using GlmSharp;
using Zeldo.Entities.Core;
using Zeldo.Entities.Player;

namespace Zeldo.Control
{
	public class WallController : AbstractController
	{
		// This is technically wasteful (since the base class stores a reference to the parent as well), but storing
		// here means the actor doesn't need to be cast repeatedly.
		private PlayerCharacter player;

		// For the time being, only the player is capable of traversing walls.
		public WallController(PlayerCharacter player) : base(player)
		{
			this.player = player;
		}

		public vec2 FlatDirection { get; set; }

		public override void PreStep(float step)
		{
		}

		public override void PostStep(float step)
		{
		}
	}
}
