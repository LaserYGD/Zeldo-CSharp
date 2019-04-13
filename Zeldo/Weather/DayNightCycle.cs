using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core;
using Engine.Interfaces;

namespace Zeldo.Weather
{
	public class DayNightCycle : IDynamic
	{
		public Color AmbientColor { get; private set; }

		public void Update(float dt)
		{
		}
	}
}
