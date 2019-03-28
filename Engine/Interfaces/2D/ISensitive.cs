using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Sensors;

namespace Engine.Interfaces._2D
{
	public interface ISensitive
	{
		void OnSense(SensorTypes sensorType, object target);
		void OnSeparate(SensorTypes sensorType, object target);
	}
}
