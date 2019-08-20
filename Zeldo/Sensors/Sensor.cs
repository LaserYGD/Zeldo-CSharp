using System;
using System.Collections.Generic;
using Engine.Interfaces._3D;
using Engine.Shapes._3D;
using GlmSharp;

namespace Zeldo.Sensors
{
	public class Sensor : ITransformable3D
	{
		public Sensor(SensorTypes type, SensorUsages usage, object parent, Shape3D shape = null, float height = 1)
		{
			SensorType = type;
			Usage = usage;
			Parent = parent;
			Shape = shape;
			IsEnabled = true;
			Contacts = new List<Sensor>();
		}

		public SensorTypes SensorType { get; }
		public SensorUsages Usage { get; set; }

		public object Parent { get; }

		public virtual vec3 Position
		{
			get => Shape.Position;
			set => Shape.Position = value;
		}

		public virtual quat Orientation
		{
			get => Shape.Orientation;
			set => Shape.Orientation = value;
		}

		public Shape3D Shape { get; set; }
		public List<Sensor> Contacts { get; }

		public Action<SensorTypes, object> OnSense { get; set; }
		public Action<SensorTypes, object> OnSeparate { get; set; }

		public bool IsEnabled { get; set; }
		public bool IsTogglePending { get; set; }
		public bool IsMarkedForDestruction { get; set; }
		public bool IsCompound { get; protected set; }

		public virtual void SetTransform(vec3 position, quat orientation)
		{
			Position = position;
			Orientation = orientation;
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
