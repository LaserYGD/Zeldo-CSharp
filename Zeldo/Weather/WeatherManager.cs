using Engine.Core;
using Engine.Interfaces;
using Engine.Shaders;

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

		public float TimeOfDay => dayNightCycle.TimeOfDay;

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
