using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core;
using Engine.Interfaces;
using Engine.Shaders;
using GlmSharp;

namespace Zeldo.Weather
{
	public class WeatherManager : IDynamic
	{
		private Shader shader;
		private DayNightCycle dayNightCycle;
		private WeatherFormation activeFormation;

		public WeatherManager()
		{
			dayNightCycle = new DayNightCycle();
		}

		public void Update(float dt)
		{
			dayNightCycle.Update(dt);

			Color lightColor = dayNightCycle.LightColor;

			if (activeFormation != null)
			{
				activeFormation.Update(dt);
				lightColor = activeFormation.FilterLight(lightColor);
			}

			shader.SetUniform("lightColor", lightColor.ToVec3());
		}
	}
}
