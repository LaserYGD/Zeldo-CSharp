using System;
using Engine.Interfaces;
using Engine.Timing;
using Zeldo.Entities.Core;

namespace Zeldo.Combat
{
	// Although most attacks are carried out by living entities, it's possible a non-living entity (like a static
	// turret) could attack as well (without requiring health values, damage callbacks, and other data associated with
	// living things).
	public abstract class Attack<T> : IComponent where T : Entity
	{
		private AttackData data;
		private SingleTimer timer;
		private Action<float>[] phaseTicks;
		private AttackPhases phase;

		protected Attack(AttackData data, T parent)
		{
			this.data = data;

			Parent = parent;

			timer = new SingleTimer(time => { AdvancePhase(); });
			timer.IsPaused = true;
			timer.IsRepeatable = true;

			phase = AttackPhases.Idle;
			phaseTicks = new Action<float>[]
			{
				WhilePreparing,
				WhileExecuting,
				WhileCooling,
				WhileResetting
			};
		}

		protected T Parent { get; }

		// Attacks are only added to component collections when started. As such, the attack is complete when it
		// loops through all phases back around to idle.
		public bool IsComplete => phase == AttackPhases.Idle;

		public void Start()
		{
			// It's assumed that if this function is called, at least one phase will be enabled. Otherwise, the phase
			// will immediately advance back to idle.
			AdvancePhase();
		}

		private void AdvancePhase()
		{
			int phaseIndex;

			do
			{
				phase = (AttackPhases)(((int)phase + 1) % 5);
				phaseIndex = (int)phase - 1;
			}
			// A phase is considered disabled if its duration is zero.
			while (phase != AttackPhases.Idle && data.Durations[phaseIndex] == 0);

			if (phase == AttackPhases.Idle)
			{
				// In this case, the timer will automatically pause itself (since the timer is marked repeatable).
				timer.Elapsed = 0;

				return;
			}

			timer.Duration = data.Durations[phaseIndex];
			timer.Tick = phaseTicks[phaseIndex];
			timer.IsPaused = false;

			switch (phase)
			{
				case AttackPhases.Prepare:
					OnPrepare();
					break;

				case AttackPhases.Execute:
					OnExecute();
					break;

				case AttackPhases.Cooldown:
					OnCooldown();
					break;

				case AttackPhases.Reset:
					OnReset();
					break;
			}
		}

		protected virtual void OnPrepare()
		{
		}

		protected virtual void WhilePreparing(float progress)
		{
		}

		protected virtual void OnExecute()
		{
		}

		protected virtual void WhileExecuting(float progress)
		{
		}

		protected virtual void OnCooldown()
		{
		}

		protected virtual void WhileCooling(float progress)
		{
		}

		protected virtual void OnReset()
		{
		}

		protected virtual void WhileResetting(float progress)
		{
		}

		public void Update(float dt)
		{
			if (phase != AttackPhases.Idle)
			{
				timer.Update(dt);
			}
		}
	}
}
