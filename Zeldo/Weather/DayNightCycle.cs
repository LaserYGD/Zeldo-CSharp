using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core;
using Engine.Interfaces;
using Engine.Shaders;
using Engine.Utility;
using GlmSharp;

namespace Zeldo.Weather
{
	public class DayNightCycle : IDynamic
	{
		private Shader shader;

		public DayNightCycle()
		{
			LightColor = Color.White;
			LightDirection = Utilities.Normalize(new vec3(-1, -0.4f, 0));
		}

		public Color LightColor { get; private set; }
		public vec3 LightDirection { get; private set; }

		public void Update(float dt)
		{
		}
	}
}
