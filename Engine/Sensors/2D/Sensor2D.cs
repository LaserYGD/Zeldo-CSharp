using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces._2D;
using Engine.Shapes._2D;

namespace Engine.Sensors._2D
{
	public class Sensor2D
	{
		public Sensor2D(SensorTypes sensorType, object owner, Shape2D shape = null)
		{
			SensorType = sensorType;
			Owner = owner;
			Shape = shape;
			Enabled = true;
			CanTouch = SensorTypes.All;
			ContactList = new List<Sensor2D>();
		}

		public SensorTypes SensorType { get; }
		public SensorTypes CanTouch { get; set; }

		public bool Enabled { get; set; }
		public object Owner { get; }

		public Shape2D Shape { get; set; }
		public List<Sensor2D> ContactList { get; }

		public Action<SensorTypes, object> OnSense { get; set; }
		public Action<SensorTypes, object> OnSeparate { get; set; }
	}
}
