using Zeldo.Combat;

namespace Zeldo.Entities.Player.Combat
{
	public class SwordSlash : Attack<PlayerCharacter>
	{
		public SwordSlash(AttackData data, PlayerCharacter parent) : base(data, parent)
		{
		}
	}
}
