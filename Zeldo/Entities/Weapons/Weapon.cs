using System.Collections.Generic;
using Engine.Timing;
using Zeldo.Combat;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Weapons
{
	// TODO: Consider moving the attack map down here (since all weapons will need attacks attached).
	public abstract class Weapon : Entity
	{
		private SingleTimer cooldownTimer;

		protected Weapon() : base(EntityGroups.Weapon)
		{
			cooldownTimer = new SingleTimer(time =>
			{
				IsCoolingDown = false;
				OnCooldownExpired();
			});

			cooldownTimer.IsRepeatable = true;
			cooldownTimer.IsPaused = true;
		}

		public Actor Owner { get; set; }

		// If a weapon is on cooldown, input may still be buffered for a short time to trigger the next attack (in the
		// case of the player, anyway).
		public bool IsCoolingDown { get; private set; }

		protected virtual void OnCooldownExpired()
		{
		}

		// This function is used to signal the player controller to trigger another attack immediately if input was
		// buffered.
		public bool HasCooldownExpired(float dt)
		{
			bool previouslyOnCooldown = !cooldownTimer.IsPaused;
			cooldownTimer.Update(dt);

			return previouslyOnCooldown && cooldownTimer.IsPaused;
		}

		// Player attacks use a short input buffering window in order to make chaining a series of attacks a bit easier
		// and more fluid. The specific length of that window is configurable per attack animation, though. To that
		// end, the return value of this function is used directly for input buffering.
		public float TriggerPrimary()
		{
			TriggerPrimary(out float cooldownTime, out float bufferTime);

			if (cooldownTime > 0)
			{
				cooldownTimer.Duration = cooldownTime;
				cooldownTimer.IsPaused = false;
			}

			return bufferTime;
		}

		protected abstract void TriggerPrimary(out float cooldownTime, out float bufferTime);
	}
}
