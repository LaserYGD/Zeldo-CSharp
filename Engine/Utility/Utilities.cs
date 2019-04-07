using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core._2D;
using GlmSharp;

namespace Engine.Utility
{
	public static class Utilities
	{
		public static int EnumCount<T>()
		{
			return Enum.GetValues(typeof(T)).Length;
		}

		public static ivec2 ComputeOrigin(int width, int height, Alignments alignment)
		{
			bool left = (alignment & Alignments.Left) > 0;
			bool right = (alignment & Alignments.Right) > 0;
			bool top = (alignment & Alignments.Top) > 0;
			bool bottom = (alignment & Alignments.Bottom) > 0;

			int x = left ? 0 : (right ? width : width / 2);
			int y = top ? 0 : (bottom ? height : height / 2);

			return new ivec2(x, y);
		}
	}
}
