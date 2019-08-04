using Engine.Core;
using Engine.Interfaces;
using Engine.Utility;

namespace Engine.Smoothers
{
	public class ColorSmoother : Smoother<Color>
	{
		private IColorable target;

		public ColorSmoother(IColorable target, Color start, Color end, float duration, EaseTypes easeType) :
			base(start, end, duration, easeType)
		{
			this.target = target;
		}

		protected override void Smooth(float t)
		{
			target.Color = Color.Lerp(Start, End, t);
		}
	}
}
