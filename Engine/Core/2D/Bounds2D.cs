using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;

namespace Engine.Core._2D
{
	public class Bounds2D
	{
		private int x;
		private int y;
		private int width;
		private int height;

		public Bounds2D() : this(0, 0, 0, 0)
		{
		}

		public Bounds2D(int width, int height) : this(0, 0, width, height)
		{
		}

		public Bounds2D(int x, int y, int width, int height)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}

		public int X
		{
			get => x;
			set => x = value;
		}

		public int Y
		{
			get => y;
			set => y = value;
		}

		public int Width
		{
			get => width;
			set => width = value;
		}

		public int Height
		{
			get => height;
			set => height = value;
		}

		public int Left
		{
			get => x;
			set => x = value;
		}

		public int Right
		{
			get => x + width - 1;
			set => x = value - width + 1;
		}

		public int Top
		{
			get => y;
			set => y = value;
		}

		public int Bottom
		{
			get => y + height - 1;
			set => y = value - height + 1;
		}

		public bool Contains(ivec2 point)
		{
			return Contains(point.x, point.y);
		}

		public bool Contains(vec2 point)
		{
			return Contains(point.x, point.y);
		}

		private bool Contains(float x, float y)
		{
			return x >= Left && x <= Right && y >= Top && y <= Bottom;
		}

		public Bounds2D Expand(int value)
		{
			return new Bounds2D(x - value, y - value, width + value * 2, height + value * 2);
		}
	}
}
