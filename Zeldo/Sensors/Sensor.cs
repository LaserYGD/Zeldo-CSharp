using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces._2D;
using Engine.Shapes._2D;
using GlmSharp;

namespace Zeldo.Sensors
{
	public class Sensor : IPositionable2D, IRotatable
	{
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

		public bool Enabled { get; set; }
		public object Owner { get; }
		public float Rotation { get; set; }

		// Sensors primarily exist on a 2D plane, but which plane they're currently on can changed as entities move
		// vertically. Using an integer for elevation is sufficient for this purpose. Height allows certain sensors to
		// catch collisions at any elevation (such as a cutscene trigger that should activate even if the player is
		// airborne).
		public int Elevation { get; set; }
		public int Height { get; set; }

		public vec2 Position { get; set; }
		public Shape2D Shape { get; set; }
		public List<Sensor> Contacts { get; }

		public Action<SensorTypes, object> OnSense { get; set; }
		public Action<SensorTypes, object> OnSeparate { get; set; }
	}
}
