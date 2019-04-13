using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Entities;
using Engine.Sensors;
using Engine.Sensors._2D;
using Engine.Shapes._2D;
using Engine.Timing;
using Engine.Utility;
using GlmSharp;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Weapons
{
	public class Sword
	{
		private Arc arc;
		private Sensor2D sensor;
		private SingleTimer timer;
		private List<ITargetable> targetsHit;

		public Sword()
		{
			arc = new Arc();
			targetsHit = new List<ITargetable>();
			sensor = new Sensor2D(SensorTypes.Zone, this, arc)
			{
				CanTouch = SensorTypes.Entity,
				Enabled = false,
				OnSense = (sensorType, owner) =>
				{
					if (sensorType == SensorTypes.Entity && owner is ITargetable target)
					{
						ApplyDamage(target);
					}
				}
			};

			timer = new SingleTimer(time =>
			{
				sensor.Enabled = false;
				timer.Elapsed = 0;
				timer.Paused = true;
				targetsHit.Clear();
			},
			0.2f);

			timer.Paused = true;
		}

		private void ApplyDamage(ITargetable target)
		{
			// The sword can only hit each target once per swing.
			if (targetsHit.Contains(target))
			{
				return;
			}

			float angle = arc.Angle;

			target.OnHit(10, 10, angle, Utilities.Direction(angle), this);
			targetsHit.Add(target);
		}

		public void Attack(vec2 direction)
		{
			arc.Angle = Utilities.Angle(direction);
			sensor.Enabled = true;
		}
	}
}
