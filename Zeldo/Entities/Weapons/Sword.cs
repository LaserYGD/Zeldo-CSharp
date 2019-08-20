using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Shapes._2D;
using Engine.Timing;
using Engine.Utility;
using GlmSharp;
using Newtonsoft.Json.Linq;
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
				sensor.IsEnabled = false;
				targetsHit.Clear();
			},
			0.2f);

			timer.Paused = true;
			timer.Repeatable = true;

			Components.Add(timer);
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

		public void Attack(vec2 direction)
		{
			arc.Angle = Utilities.Angle(direction);
			sensor.IsEnabled = true;
			timer.Paused = false;
		}
	}
}
