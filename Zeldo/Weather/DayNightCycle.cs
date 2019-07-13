using System;
using Engine;
using Engine.Core;
using Engine.Interfaces;
using Engine.Shaders;
using Engine.Utility;
using GlmSharp;

namespace Zeldo.Weather
{
	public class DayNightCycle : IDynamic, IDisposable
	{
		private Shader shader;

		private int dayDuration;

		public DayNightCycle()
		{
			// Day duration is listed in minutes in the property file.
			dayDuration = Properties.GetInt("day.duration") * 60;
			
			LightColor = Color.White;
			LightDirection = Utilities.Normalize(new vec3(-1, -0.4f, 0));
		}

		public float TimeOfDay { get; private set; }

		public Color LightColor { get; private set; }
		public vec3 LightDirection { get; private set; }

		public void Dispose()
		{
			shader.Dispose();
		}

		public void Update(float dt)
		{
			TimeOfDay += dt;

			if (TimeOfDay >= dayDuration)
			{
				TimeOfDay -= dayDuration;
			}
		}
	}
}
