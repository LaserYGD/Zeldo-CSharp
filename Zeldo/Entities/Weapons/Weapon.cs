using System.Diagnostics;
using Engine.Core;
using Zeldo.Combat;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Weapons
{
	// TODO: Consider moving the attack map down here (since all weapons will need attacks attached).
	public abstract class Weapon<T> : Entity where T : LivingEntity
	{
		private AttackCollection<T> attacks;

		// The active attack is stored so that it can be canceled.
		private Attack<T> activeAttack;

		// TODO: Should weapons be swappable? (would require that owner be changeable)
		protected Weapon(string attackFile, T owner) : base(EntityGroups.Weapon)
		{
			Debug.Assert(owner != null, "Weapons must have a non-null owner.");

			Owner = owner;
			attacks = new AttackCollection<T>(attackFile, owner);
		}

		protected Attack<T> ActiveAttack => activeAttack;

		public T Owner { get; }

		// TODO: Revisit how weapon cooldown works (primarily for the player, but might be applicable to other actors as well).
		// If a weapon is on cooldown, input may still be buffered for a short time to trigger the next attack (in the
		// case of the player, anyway).
		public bool IsCoolingDown => activeAttack != null && activeAttack.IsCoolingDown;

		protected virtual void OnCooldownExpired()
		{
		}

		// This function is used to signal the player controller to trigger another attack immediately if input was
		// buffered.
		/*
		public bool HasCooldownExpired(float dt)
		{
			bool previouslyOnCooldown = IsCoolingDown;
			cooldownFlag.Update(dt);

			return previouslyOnCooldown && !cooldownFlag.Value;
		}
		*/

		// Player attacks use a short input buffering window in order to make chaining a series of attacks a bit easier
		// and more fluid. The specific length of that window is configurable per attack animation, though. To that
		// end, the return value of this function is used directly for input buffering.
		// TODO: Implement buffer time for player attacks (configurable per-attack).
		public Attack<T> TriggerPrimary()
		{
			activeAttack?.Cancel();
			activeAttack = attacks.Execute();
			activeAttack.Start();

			return activeAttack;
		}

		// This is primarily (maybe only) used by the player (for example, for releasing a taut bowstring).
		public virtual void ReleasePrimary()
		{
		}
	}
}
