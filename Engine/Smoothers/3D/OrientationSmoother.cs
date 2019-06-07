using Engine.Interfaces._3D;
using Engine.Utility;
using GlmSharp;

namespace Engine.Smoothers._3D
{
	public class OrientationSmoother : Smoother<quat>
	{
		private IOrientable target;

		public OrientationSmoother(IOrientable target, quat start, quat end, float duration, EaseTypes easeType) :
			base(start, end, duration, easeType)
		{
			this.target = target;
		}

		protected override void Smooth(float t)
		{
			target.Orientation = t == 1 ? End : quat.SLerp(Start, End, t);
		}
	}
}
