using System;
using Engine.Interfaces;

namespace Zeldo.Weather
{
	public abstract class WeatherFormation : IDynamic, IDisposable
	{
		public float Intensity { get; set; }

		public virtual void Dispose()
		{
		}

		public abstract void Update(float dt);
	}
}
