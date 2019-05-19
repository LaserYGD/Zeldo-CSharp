using GlmSharp;
using Zeldo.Entities.Core;
using Zeldo.Entities.Projectiles;

namespace Zeldo.Entities.Weapons
{
	public class Bow : Entity
	{
		public Bow() : base(EntityGroups.Weapon)
		{
		}

		public void PrimaryAttack(vec2 direction, float angle)
		{
			Arrow arrow = new Arrow();
			arrow.Position = Position + new vec3(0, 0.5f, 0);
			arrow.Orientation = quat.FromAxisAngle(angle, -vec3.UnitY);

			Scene.Add(arrow);
		}
	}
}
