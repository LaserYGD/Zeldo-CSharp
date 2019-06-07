using System;
using System.Collections.Generic;

namespace Zeldo.Sensors
{
	public class Space
	{
		private List<Sensor> sensors;

		// The "main loop" refers to the first loop in the Update function that processes sensor intersection and
		// separation. While that loop is active, sensors can't be removed directly, but are instead marked for
		// destruction following the loop.
		private bool mainLoopActive;

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
			if (mainLoopActive)
			{
				sensor.IsMarkedForDestruction = true;
			}
			else
			{
				sensor.ClearContacts();
				sensors.Remove(sensor);
			}
		}

		public void Update()
		{
			mainLoopActive = true;

			for (int i = 0; i < sensors.Count; i++)
			{
				var sensor1 = sensors[i];

				if (!sensor1.IsEnabled)
				{
					continue;
				}

				var contacts1 = sensor1.Contacts;

				for (int j = i + 1; j < sensors.Count; j++)
				{
					var sensor2 = sensors[j];

					if (!sensor2.IsEnabled)
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

			mainLoopActive = false;

			// Process changes (either from toggling the enabled flag or removing sensors marked for destruction).
			for (int i = sensors.Count - 1; i >= 0; i--)
			{
				var sensor = sensors[i];

				if (sensor.IsMarkedForDestruction)
				{
					sensor.ClearContacts();
					sensors.RemoveAt(i);

					continue;
				}

				if (sensor.IsTogglePending)
				{
					sensor.IsEnabled = !sensor.IsEnabled;

					if (!sensor.IsEnabled)
					{
						sensor.ClearContacts();
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
