using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Core
{
	public abstract class LivingEntity : Entity, ITargetable
	{
		private int health;
		private int maxHealth;

		protected LivingEntity(EntityGroups group) : base(group)
		{
		}

		public virtual void OnHit(int damage, int knockback, float angle, vec2 direction, object source)
		{
		}

		protected virtual void OnDeath()
		{
		}
	}
}
