using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces._2D;
using Engine.Interfaces._3D;
using Engine.Shapes._2D;
using GlmSharp;

namespace Zeldo.Sensors
{
	public class Sensor : IPositionable3D, IRotatable, IDisposable
	{
		private bool enabled;

		public Sensor(SensorTypes type, object owner, Shape2D shape = null, int height = 1)
		{
			SensorType = type;
			Owner = owner;
			Shape = shape;
			Height = height;
			Enabled = true;
			Contacts = new List<Sensor>();
		}

		public SensorTypes SensorType { get; }

		public bool Enabled
		{
			get => enabled;
			set
			{
				if (enabled != value)
				{
					if (!value)
					{
						ClearContacts();
					}

					enabled = value;
				}
			}
		}

		public object Owner { get; }

		public float Rotation
		{
			get => Shape.Rotation;
			set => Shape.Rotation = value;
		}

		// Sensors primarily exist on a 2D plane, but which plane they're currently on can changed as entities move
		// vertically. Using an integer for elevation is sufficient for this purpose. Height allows certain sensors to
		// catch collisions at any elevation (such as a cutscene trigger that should activate even if the player is
		// airborne).
		public int Elevation { get; set; }
		public int Height { get; set; }

		public vec3 Position
		{
			get
			{
				vec2 p = Shape.Position;

				return new vec3(p.x, Elevation, p.y);
			}
			set
			{
				Shape.Position = new vec2(value.x, value.z);
				Elevation = (int)value.y;
			}
		}

		public Shape2D Shape { get; set; }
		public List<Sensor> Contacts { get; }

		public Action<SensorTypes, object> OnSense { get; set; }
		public Action<SensorTypes, object> OnSeparate { get; set; }

		public void Dispose()
		{
			ClearContacts();
		}

		private void ClearContacts()
		{
			foreach (Sensor sensor in Contacts)
			{
				sensor.Contacts.Remove(this);
				sensor.OnSeparate?.Invoke(SensorType, Owner);
			}

			Contacts.Clear();
		}
	}
}
