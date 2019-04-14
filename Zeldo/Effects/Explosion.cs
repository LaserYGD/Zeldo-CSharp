using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Shapes._2D;
using Zeldo.Interfaces;
using Zeldo.Sensors;

namespace Zeldo.Effects
{
	public class Explosion
	{
		private Circle circle;
		private Sensor sensor;

		public Explosion(float radius)
		{
			circle = new Circle(radius);
			sensor = new Sensor(SensorTypes.Zone, this, circle)
			{
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
