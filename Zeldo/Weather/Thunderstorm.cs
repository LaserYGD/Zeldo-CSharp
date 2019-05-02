using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core;

namespace Zeldo.Weather
{
	public class Thunderstorm : WeatherFormation
	{
		private bool lightningVisible;

		public override Color FilterLight(Color color)
		{
			return lightningVisible ? Color.White : color;
		}

		public override void Dispose()
		{
		}

		public override void Update(float dt)
		{
		}
	}
}
