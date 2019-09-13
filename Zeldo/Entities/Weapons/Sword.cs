using System.Collections.Generic;
using Engine.Sensors;
using Engine.Shapes._2D;
using Engine.Timing;
using Engine.Utility;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Weapons
{
	public class Sword : Weapon
	{
		private Arc arc;
		private Sensor sensor;

		public Sword()
		{
			arc = new Arc(1, 1.25f);
		}

		public override void Initialize(Scene scene, JToken data)
		{
			sensor = null;//CreateSensor(scene, arc, SensorUsages.Hitbox);
			sensor.IsEnabled = false;
			sensor.OnSense = (sensorType, owner) =>
			{
				if (sensorType == SensorTypes.Entity && owner is ITargetable target)
				{
					ApplyDamage(target);
				}
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

			float angle = arc.Angle;

			target.OnHit(3, 10, angle, Utilities.Direction(angle), this);
			targetsHit.Add(target);
		}

		protected override void TriggerPrimary(out float cooldownTime, out float bufferTime)
		{
			/*
			arc.Angle = Utilities.Angle(direction);
			sensor.IsEnabled = true;
			timer.IsPaused = false;
			*/
		}
	}
}
