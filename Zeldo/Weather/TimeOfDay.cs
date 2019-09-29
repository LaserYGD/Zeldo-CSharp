using System;
using Engine;
using Engine.Core;
using Engine.Interfaces;
using Engine.Shaders;
using Engine.Utility;
using GlmSharp;

namespace Zeldo.Weather
{
	public class TimeOfDay : IDynamic, IDisposable
	{
		private Shader shader;

		private int dayDuration;
		private int[] horizon;

		// TODO: Load texture data (then clear the texture).
		public TimeOfDay()
		{
			// Day duration is listed in minutes in the property file.
			dayDuration = Properties.GetInt("day.duration") * 60;

			var texture = ContentCache.GetTexture("Horizon.png", true, false);
			horizon = texture.Data;
			
			LightColor = Color.White;
			LightDirection = Utilities.Normalize(new vec3(-1, -0.4f, 0));
		}

		public float Time { get; private set; }

		// TODO: Update light as time of day progresses (ambient light too).
		public Color LightColor { get; private set; }
		public vec3 LightDirection { get; private set; }

		public void Dispose()
		{
			shader.Dispose();
		}

		public void Update(float dt)
		{
			Time += dt;

			if (Time >= dayDuration)
			{
				Time -= dayDuration;
			}
		}
	}
}
