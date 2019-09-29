using Engine.Core;
using Engine.Interfaces;
using Engine.Shaders;

namespace Zeldo.Weather
{
	public class WeatherManager : IDynamic
	{
		private Shader shader;
		private TimeOfDay dayNightCycle;
		private WeatherFormation activeFormation;

		public WeatherManager()
		{
			dayNightCycle = new TimeOfDay();
		}

		public float TimeOfDay => dayNightCycle.Time;

		public void Update(float dt)
		{
			dayNightCycle.Update(dt);

			Color lightColor = dayNightCycle.LightColor;

			if (activeFormation != null)
			{
				activeFormation.Update(dt);
				//lightColor = activeFormation.FilterLight(lightColor);
			}

			shader.SetUniform("lightColor", lightColor.ToVec3());
		}
	}
}
