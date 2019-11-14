using System;
using System.Diagnostics;
using Engine.Interfaces;
using Engine.Timing;
using Zeldo.Entities.Core;

namespace Zeldo.Combat
{
	public abstract class Attack<T> : IComponent where T : LivingEntity
	{
		private AttackData data;
		private SingleTimer timer;
		private Action<float>[] phaseTicks;
		private Func<bool> triggerCallback;
		private AttackPhases phase;

		// If phase advancement is stalled, ShouldAdvance is called every frame until the phase is allowed to advance.
		private bool isWaitingOnPhase;

		protected Attack(AttackData data, T parent)
		{
			this.data = data;

			Parent = parent;
			timer = new SingleTimer(t =>
			{
				if (ShouldAdvance(phase))
				{
					AdvancePhase();
				}
				else
				{
					isWaitingOnPhase = true;
				}
			});

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
		public bool IsComplete => phase == AttackPhases.Complete;
		public bool IsCoolingDown => phase == AttackPhases.Cooldown;

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

		public virtual void Cancel()
		{
			phase = AttackPhases.Complete;
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

			if (phase == AttackPhases.Complete)
			{
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

		// This is useful when certain phases need to be stalled while waiting for another event to occur (e.g. once
		// the bow is drawn, an attack isn't executed until the button is released).
		protected virtual bool ShouldAdvance(AttackPhases phase)
		{
			return true;
		}

		protected virtual void OnPrepare()
		{
		}

		protected virtual void WhilePreparing(float t)
		{
		}

		protected virtual void OnExecute()
		{
		}

		protected virtual void WhileExecuting(float t)
		{
		}

		protected virtual void OnCooldown()
		{
		}

		protected virtual void WhileCooling(float t)
		{
		}

		protected virtual void OnReset()
		{
		}

		protected virtual void WhileResetting(float t)
		{
		}

		public void Update(float dt)
		{
			if (isWaitingOnPhase && ShouldAdvance(phase))
			{
				AdvancePhase();
				isWaitingOnPhase = false;

				// The timer shouldn't be updated again if the phase was just advanced.
				return;
			}

			if (phase != AttackPhases.Idle)
			{
				timer.Update(dt);
			}
		}
	}
}
