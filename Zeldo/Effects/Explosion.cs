using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Sensors;
using Engine.Sensors._2D;
using Engine.Shapes._2D;
using Zeldo.Interfaces;

namespace Zeldo.Effects
{
	public class Explosion
	{
		private Circle circle;
		private Sensor2D sensor;

		public Explosion(float radius)
		{
			circle = new Circle(radius);
			sensor = new Sensor2D(SensorTypes.Zone, this, circle)
			{
				CanTouch = SensorTypes.Entity,
				OnSense = (sensorType, owner) =>
				{
					if (owner is ITargetable target)
					{
						ApplyDamage(target);
					}
				}
			};
		}

		private void ApplyDamage(ITargetable target)
		{
		}
	}
}
