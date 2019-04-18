using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core;
using Engine.Interfaces;

namespace Zeldo.Weather
{
	public abstract class WeatherFormation : IDynamic
	{
		public virtual Color FilterLight(Color color)
		{
			return color;
		}

		public abstract void Update(float dt);
	}
}
