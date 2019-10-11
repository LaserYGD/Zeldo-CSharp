using System;
using System.Diagnostics;
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
		private Func<bool> triggerCallback;
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

		// Like many components, attacks are designed to be instantiated once when the parent is spawned, then
		// activated when needed.
		public bool IsComplete => false;

		// All attacks have a specific list of requirements before they'll execute. Sometimes this logic is simple and
		// can be encapsulated in the attack class itself. For cases when trigger logic is more complicated, a custom
		// callback can be attached instead.
		public Func<bool> TriggerCallback
		{
			set
			{
				Debug.Assert(value != null, "Can't set a null trigger callback on attacks.");

				triggerCallback = value;
			}
		}

		// Attack logic assumes that, for any particular set of attacks and under any possible circumstances, there
		// will be exactly one attack eligible to be executed. This design also intentionally makes it more difficult
		// to implement RNG in attack patterns.
		public virtual bool ShouldTrigger()
		{
			// This means that if a callback isn't set and this function isn't overridden, attack classes cannot
			// execute (since this function will always return false).
			return triggerCallback != null && triggerCallback();
		}

		public void Start()
		{
			// All attacks are guaranteed to have at least one phase enabled (the execution phase).
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
