using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Zeldo.Combat;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Weapons
{
	public class Sword : MeleeWeapon
	{
		// TODO: Move most (or all) of this logic down to the base MeleeWeapon class.
		// Melee weapons only hit targets once per swing (even if sensors overlap for multiple frames).
		private List<ITargetable> targetsHit;
		private Dictionary<string, AttackData> attacks;

		public Sword()
		{
			targetsHit = new List<ITargetable>();
			attacks = AttackData.Load("PlayerSwordAttacks.json");
		}

		public override void Initialize(Scene scene, JToken data)
		{
			var sensor = CreateSensor(scene, null, SensorGroups.DamageSource);
			sensor.Affects = (int)SensorGroups.Target;
			sensor.IsEnabled = false;
			sensor.OnSense = (sensorType, owner) =>
			{
				ApplyDamage((ITargetable)owner);
			};

			base.Initialize(scene, data);
		}

		private void ApplyDamage(ITargetable target)
		{
			// The sword can only hit each target once per swing.
			if (targetsHit.Contains(target))
			{
				return;
			}

			//float angle = arc.Angle;

			//target.OnHit(3, 10, angle, Utilities.Direction(angle), this);
			targetsHit.Add(target);
		}

		protected override void OnCooldownExpired()
		{
			targetsHit.Clear();
		}

		protected override void TriggerPrimary(out float cooldownTime, out float bufferTime)
		{
			cooldownTime = 1000;
			bufferTime = 0;
		}
	}
}
