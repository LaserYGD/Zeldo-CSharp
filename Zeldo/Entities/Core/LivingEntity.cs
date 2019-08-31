using System;
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

		public int Health
		{
			get => health;
			set
			{
				if (health != value)
				{
					int oldHealth = health;
					health = value;
					OnHealthChange(oldHealth, health);
				}
			}
		}

		public int MaxHealth
		{
			get => maxHealth;
			set
			{
				if (maxHealth != value)
				{
					int oldMax = maxHealth;
					maxHealth = value;
					OnMaxHealthChange(oldMax, maxHealth);
				}
			}
		}

		public virtual void OnHit(int damage, int knockback, float angle, vec2 direction, object source)
		{
			// TODO: Should the function return early if health is already zero?
			Health = Math.Max(health - damage, 0);
			
			if (health == 0)
			{
				OnDeath();
			}
		}

		protected virtual void OnHealthChange(int oldHealth, int newHealth)
		{
		}

		protected virtual void OnMaxHealthChange(int oldMax, int newMax)
		{
		}

		protected virtual void OnDeath()
		{
		}
	}
}
