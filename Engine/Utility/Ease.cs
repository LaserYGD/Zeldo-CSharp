using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Utility
{
	public enum EaseTypes
	{
		Linear,
		QuadraticIn,
		QuadraticOut,
		QuadraticInOut,
		CubicIn,
		CubicOut
	}

	// See https://github.com/acron0/Easings/blob/master/Easings.cs.
	public static class Ease
	{
		public static float Compute(float t, EaseTypes easeType)
		{
			switch (easeType)
			{
				case EaseTypes.QuadraticIn: return QuadraticIn(t);
				case EaseTypes.QuadraticOut: return QuadraticOut(t);
				case EaseTypes.QuadraticInOut: return QuadraticInOut(t);
				case EaseTypes.CubicIn: return CubicIn(t);
				case EaseTypes.CubicOut: return CubicOut(t);
			}

			// This is equivalent to EaseTypes.Linear.
			return t;
		}

		private static float QuadraticIn(float t)
		{
			return t * t;
		}

		private static float QuadraticOut(float t)
		{
			return -(t * (t - 2));
		}

		private static float QuadraticInOut(float t)
		{
			if (t < 0.5f)
			{
				return t * t * 2;
			}

			return  t * t * -2 + t * 4 - 1;
		}

		private static float CubicIn(float t)
		{
			return t * t * t;
		}

		private static float CubicOut(float t)
		{
			float f = t - 1;

			return f * f * f + 1;
		}
	}
}
