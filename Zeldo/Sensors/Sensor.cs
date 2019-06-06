using System;
using System.Collections.Generic;
using Engine.Shapes._2D;
using GlmSharp;
using Zeldo.Interfaces;

namespace Zeldo.Sensors
{
	public class Sensor : ITransformable2D, IDisposable
	{
		private bool enabled;

		public Sensor(SensorTypes type, object owner, Shape2D shape = null, int height = 1)
		{
			SensorType = type;
			Owner = owner;
			Shape = shape;
			Height = height;
			IsEnabled = true;
			Contacts = new List<Sensor>();
		}

		public SensorTypes SensorType { get; }

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
		public float Elevation { get; set; }
		public float Height { get; set; }

		public vec2 Position
		{
			get => Shape.Position;
			set => Shape.Position = value;
		}

		public Shape2D Shape { get; set; }
		public List<Sensor> Contacts { get; }

		public Action<SensorTypes, object> OnSense { get; set; }
		public Action<SensorTypes, object> OnSeparate { get; set; }

		public bool IsEnabled
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

		public void SetTransform(vec2 position, float elevation, float rotation)
		{
		}

		public void Dispose()
		{
			ClearContacts();
			IsEnabled = false;
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
