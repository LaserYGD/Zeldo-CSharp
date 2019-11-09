using GlmSharp;

namespace Zeldo.Interfaces
{
	public interface ITargetable
	{
		// TODO: Should damage source (an object) be replaced with an interface?
		void OnDamage(int damage, object source = null);
		void OnHit(int damage, int knockback, float angle, vec2 direction, object source = null);
	}
}
