using GlmSharp;

namespace Zeldo.Interfaces
{
	public interface ITargetable
	{
		void OnHit(int damage, int knockback, float angle, vec2 direction, object source);
	}
}
