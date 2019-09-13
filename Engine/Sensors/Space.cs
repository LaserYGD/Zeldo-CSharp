using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Engine.Sensors
{
	public class Space
	{
		private List<Sensor> sensors;

		public Space()
		{
			sensors = new List<Sensor>();
		}

		// If the update function is currently active, sensors can't be disabled or removed directly. Instead, they're
		// marked to be toggled or destoyred following the loop.
		internal bool IsUpdateActive { get; private set; }

		public List<Sensor> Sensors => sensors;

		public void Add(Sensor sensor)
		{
			Debug.Assert(sensor != null, "Can't add a null sensor.");

			sensors.Add(sensor);
			sensor.Space = this;
		}

		public void Remove(Sensor sensor)
		{
			Debug.Assert(sensor != null, "Can't remove a null sensor.");

			if (IsUpdateActive)
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
			IsUpdateActive = true;

			// Loop through sensors to update contacts (and call callbacks as appropriate).
			for (int i = 0; i < sensors.Count; i++)
			{
				var sensor1 = sensors[i];

				Debug.Assert(sensor1.Shape != null, "Sensor never had its shape set.");

				if (!sensor1.IsEnabled)
				{
					continue;
				}

				var owner1 = sensor1.Owner;
				var type1 = sensor1.Type;
				var groups1 = sensor1.Groups;
				var affects1 = sensor1.Affects;
				var contacts1 = sensor1.Contacts;

				bool isZone1 = sensor1.Type == SensorTypes.Zone;

				for (int j = i + 1; j < sensors.Count; j++)
				{
					var sensor2 = sensors[j];

					Debug.Assert(sensor2.Shape != null, "Sensor never had its shape set.");

					// Although contacts are symmetric, callbacks can be configured to trigger in only one direction.
					bool oneAffectsTwo = (affects1 & sensor2.Groups) > 0;
					bool twoAffectsOne = (sensor2.Affects & groups1) > 0;

					// By design, zones cannot interact with each other (only entity-entity or entity-zone collisions
					// are allowed).
					if (!sensor2.IsEnabled || (isZone1 && sensor2.Type == SensorTypes.Zone) || !(oneAffectsTwo ||
						twoAffectsOne))
					{
						continue;
					}

					var owner2 = sensor2.Owner;
					var type2 = sensor2.Type;
					var contacts2 = sensor2.Contacts;

					bool overlaps = Overlaps(sensor1, sensor2);

					// Contacts are symmetric.
					if (contacts1.Contains(sensor2))
					{
						if (overlaps)
						{
							if (oneAffectsTwo) { sensor1.OnStay?.Invoke(type2, owner2); }
							if (twoAffectsOne) { sensor2.OnStay?.Invoke(type1, owner1); }
						}
						else
						{
							contacts1.Remove(sensor2);
							contacts2.Remove(sensor1);

							if (oneAffectsTwo) { sensor1.OnSeparate?.Invoke(type2, owner2); }
							if (twoAffectsOne) { sensor2.OnSeparate?.Invoke(type1, owner1); }
						}

						continue;
					}

					if (overlaps)
					{
						contacts1.Add(sensor2);
						contacts2.Add(sensor1);

						if (oneAffectsTwo) { sensor1.OnSense?.Invoke(type2, owner2); }
						if (twoAffectsOne) { sensor2.OnSense?.Invoke(type1, owner1); }
					}
				}
			}

			IsUpdateActive = false;
			ProcessChange();
		}

		private bool Overlaps(Sensor sensor1, Sensor sensor2)
		{
			bool isCompound1 = sensor1.IsCompound;
			bool isCompound2 = sensor2.IsCompound;

			// Both sensors are compound.
			if (isCompound1 && isCompound2)
			{
				var list1 = ((MultiSensor)sensor1).Attachments;
				var list2 = ((MultiSensor)sensor2).Attachments;

				foreach (var a1 in list1)
				{
					var shape1 = a1.Shape;

					foreach (var a2 in list2)
					{
						if (a2.Shape.Overlaps(shape1))
						{
							return true;
						}
					}
				}
			}

			// Only one sensor is compound.
			if (isCompound1 || isCompound2)
			{
				var multiSensor = (MultiSensor)(isCompound1 ? sensor1 : sensor2);
				var other = isCompound1 ? sensor2 : sensor1;

				return multiSensor.Attachments.Any(a => a.Shape.Overlaps(other.Shape));
			}

			// Neither sensor is compound.
			return sensor1.Shape.Overlaps(sensor2.Shape);
		}

		private void ProcessChange()
		{
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
	}
}
