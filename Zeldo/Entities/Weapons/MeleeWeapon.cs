using System.Collections.Generic;
using Engine.Sensors;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Weapons
{
	public abstract class MeleeWeapon : Weapon
	{
		// Melee weapons only hit targets once per swing (even if sensors overlap for multiple frames).
		private List<ITargetable> targetsHit;

		protected MeleeWeapon()
		{
			targetsHit = new List<ITargetable>();
		}

		public override void Initialize(Scene scene, JToken data)
		{
			var sensor = CreateSensor(scene, null, SensorGroups.DamageSource, SensorTypes.Zone);
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
	}
}
