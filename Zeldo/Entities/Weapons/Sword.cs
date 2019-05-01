using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Shapes._2D;
using Engine.Timing;
using Engine.Utility;
using GlmSharp;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;
using Zeldo.Sensors;

namespace Zeldo.Entities.Weapons
{
	public class Sword : Entity
	{
		private Arc arc;
		private Sensor sensor;
		private SingleTimer timer;
		private List<ITargetable> targetsHit;

		public Sword() : base(EntityGroups.Weapon)
		{
			arc = new Arc(1, 1.25f);
			targetsHit = new List<ITargetable>();

			timer = new SingleTimer(time =>
			{
				sensor.Enabled = false;
				targetsHit.Clear();
			},
			0.2f);

			timer.Paused = true;
			timer.Repeatable = true;

			Components.Add(timer);
		}

		public Sensor Sensor => sensor;

		public override void Initialize()
		{
			sensor = CreateSensor(arc, false);
			sensor.OnSense = (sensorType, owner) =>
			{
				if (sensorType == SensorTypes.Entity && owner is ITargetable target)
				{
					ApplyDamage(target);
				}
			};
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

		public void Attack(vec2 direction)
		{
			arc.Angle = Utilities.Angle(direction);
			sensor.Enabled = true;
			timer.Paused = false;
		}
	}
}
