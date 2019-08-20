using System;
using System.Collections.Generic;
using Engine.Shapes._2D;

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

		public List<Sensor> Sensors => sensors;

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

				var usage1 = sensor1.Usage;
				var contacts1 = sensor1.Contacts;

				bool isZone = sensor1.SensorType == SensorTypes.Zone;

				for (int j = i + 1; j < sensors.Count; j++)
				{
					var sensor2 = sensors[j];

					// By design, zone sensors are meant to never interact directly. Entities can interact with other
					// entities, though.
					if (!sensor2.IsEnabled || (isZone && sensor2.SensorType == SensorTypes.Zone) ||
					    (usage1 & sensor2.Usage) == 0)
					{
						continue;
					}

					var contacts2 = sensor2.Contacts;

					bool intersects = false;//CheckIntersection(sensor1, sensor2);

					// This contact relationship is always symmetric (i.e. if sensor A contains sensor B, then B also
					// contains A).
					if (contacts1.Contains(sensor2))
					{
						if (!intersects)
						{
							contacts1.Remove(sensor2);
							contacts2.Remove(sensor1);

							sensor1.OnSeparate?.Invoke(sensor2.SensorType, sensor2.Parent);
							sensor2.OnSeparate?.Invoke(sensor1.SensorType, sensor1.Parent);
						}

						continue;
					}

					if (intersects)
					{
						contacts1.Add(sensor2);
						contacts2.Add(sensor1);

						sensor1.OnSense?.Invoke(sensor2.SensorType, sensor2.Parent);
						sensor2.OnSense?.Invoke(sensor1.SensorType, sensor1.Parent);
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

		/*
		private bool CheckIntersection(Sensor sensor1, Sensor sensor2)
		{
			// For the time being, it's assumed that compound sensors will never interact.
			if (sensor1.IsCompound)
			{
				return CheckCompoundIntersection((CompoundSensor)sensor1, sensor2);
			}

			if (sensor2.IsCompound)
			{
				return CheckCompoundIntersection((CompoundSensor)sensor2, sensor1);
			}

			float e1 = sensor1.Elevation;
			float h1 = sensor1.Height;

			float e2 = sensor2.Elevation;
			float h2 = sensor2.Height;

			if (e1 != e2)
			{
				float delta = Math.Abs(e1 - e2);
				float halfSum = (h1 + h2) / 2;

				if (delta > halfSum)
				{
					return false;
				}
			}

			return sensor1.Shape.Overlaps(sensor2.Shape);
		}

		private bool CheckCompoundIntersection(CompoundSensor compound, Sensor other)
		{
			float baseElevation = compound.Elevation;
			float h = other.Height;
			float e = other.Elevation;

			Shape2D shape = other.Shape;

			foreach (var attachment in compound.Attachments)
			{
				float delta = Math.Abs(e - (baseElevation + attachment.Elevation));
				float halfSum = (h + attachment.Height) / 2;

				if (delta <= halfSum && shape.Overlaps(attachment.Shape))
				{
					return true;
				}
			}

			return false;
		}
		*/
	}
}
