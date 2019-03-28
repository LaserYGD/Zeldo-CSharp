using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces._2D;
using Engine.Shapes._2D;

namespace Engine.Sensors
{
	public class Sensor
	{
		public Sensor(SensorTypes sensorType, ISensitive owner)
		{
			Owner = owner;
			SensorType = sensorType;
			ContactList = new List<Sensor>();
		}

		public SensorTypes SensorType { get; }
		public ISensitive Owner { get; }

		public Shape2D Shape { get; set; }
		public List<Sensor> ContactList { get; }
	}
}
