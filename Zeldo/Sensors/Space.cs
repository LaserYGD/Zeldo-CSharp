using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Shapes._2D;

namespace Zeldo.Sensors
{
	public class Space
	{
		private List<Sensor> sensors;

		public Space()
		{
			sensors = new List<Sensor>();
		}

		public void Add(Sensor sensor)
		{
			sensors.Add(sensor);
		}

		public void Remove(Sensor sensor)
		{
			sensor.Dispose();
			sensors.Remove(sensor);
		}

		public void Update()
		{
			foreach (Sensor sensor1 in sensors)
			{
				if (!sensor1.IsEnabled)
				{
					continue;
				}

				var contacts1 = sensor1.Contacts;

				foreach (Sensor sensor2 in sensors)
				{
					if (sensor1 == sensor2 || !sensor2.IsEnabled)
					{
						continue;
					}

					var contacts2 = sensor2.Contacts;

					bool intersects = CheckIntersection(sensor1, sensor2);

					// This contact relationship is always symmetric (i.e. if sensor A contains sensor B, then B also
					// contains A).
					if (contacts1.Contains(sensor2))
					{
						if (!intersects)
						{
							contacts1.Remove(sensor2);
							contacts2.Remove(sensor1);

							sensor1.OnSeparate?.Invoke(sensor2.SensorType, sensor2.Owner);
							sensor2.OnSeparate?.Invoke(sensor1.SensorType, sensor1.Owner);
						}

						continue;
					}

					if (intersects)
					{
						contacts1.Add(sensor2);
						contacts2.Add(sensor1);

						sensor1.OnSense?.Invoke(sensor2.SensorType, sensor2.Owner);
						sensor2.OnSense?.Invoke(sensor1.SensorType, sensor1.Owner);
					}
				}
			}
		}

		private bool CheckIntersection(Sensor sensor1, Sensor sensor2)
		{
			float e1 = sensor1.Elevation;
			float h1 = sensor1.Height;

			float e2 = sensor2.Elevation;
			float h2 = sensor2.Height;

			if (e1 != e2)
			{
				float delta = Math.Abs(e1 - e2);
				float halfSum = (h1 + h2) / 2f;

				if (delta > halfSum)
				{
					return false;
				}
			}

			return sensor1.Shape.Overlaps(sensor2.Shape);
		}
	}
}
