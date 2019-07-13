using Engine.Core;

namespace Zeldo.Weather
{
	public class Thunderstorm : WeatherFormation
	{
		private bool isLightningVisible;

		public Color FilterLight(Color color)
		{
			return isLightningVisible ? Color.White : color;
		}

		public override void Dispose()
		{
		}

		public override void Update(float dt)
		{
		}
	}
}
