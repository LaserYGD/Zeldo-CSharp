using System;
using System.Collections.Generic;
using Engine.Interfaces;
using Engine.Shapes._2D;
using GlmSharp;
using Zeldo.Interfaces;

namespace Zeldo.Sensors
{
	public class Sensor : ITransformable2D
	{
		public Sensor(SensorTypes type, SensorUsages usage, object parent, Shape2D shape = null, float height = 1)
		{
			SensorType = type;
			Usage = usage;
			Parent = parent;
			Shape = shape;
			Height = height;
			IsEnabled = true;
			Contacts = new List<Sensor>();
		}

		public SensorTypes SensorType { get; }
		public SensorUsages Usage { get; set; }

		public object Parent { get; }

		public virtual float Rotation
		{
			get => Shape.Rotation;
			set => Shape.Rotation = value;
		}

		// Sensors primarily exist on a 2D plane, but which plane they're currently on can changed as entities move
		// vertically. Using an integer for elevation is sufficient for this purpose. Height allows certain sensors to
		// catch collisions at any elevation (such as a cutscene trigger that should activate even if the player is
		// airborne).
		public virtual float Elevation { get; set; }

		public float Height { get; set; }

		public virtual vec2 Position
		{
			get => Shape.Position;
			set => Shape.Position = value;
		}

		public Shape2D Shape { get; set; }
		public List<Sensor> Contacts { get; }

		public Action<SensorTypes, object> OnSense { get; set; }
		public Action<SensorTypes, object> OnSeparate { get; set; }

		public bool IsEnabled { get; set; }
		public bool IsTogglePending { get; set; }
		public bool IsMarkedForDestruction { get; set; }
		public bool IsCompound { get; protected set; }

		public virtual void SetTransform(vec2 position, float elevation, float rotation)
		{
			Position = position;
			Elevation = elevation;
			Rotation = rotation;
		}

	    public void ClearContacts()
	    {
	        foreach (Sensor other in Contacts)
	        {
				OnSeparate?.Invoke(other.SensorType, other.Parent);

		        other.OnSeparate?.Invoke(SensorType, Parent);
		        other.Contacts.Remove(this);
			}

	        Contacts.Clear();
        }
	}
}
