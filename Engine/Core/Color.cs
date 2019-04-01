using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Core
{
	public class Color
	{
		public static readonly Color Black = new Color(0);
		public static readonly Color White = new Color(255);

		private byte r;
		private byte g;
		private byte b;
		private byte a;

		public Color(byte value) : this(value, value, value, 255)
		{
		}

		public Color(byte r, byte g, byte b) : this(r, g, b, 255)
		{
		}

		public Color(byte r, byte g, byte b, byte a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public float ToFloat()
		{
			return BitConverter.ToSingle(new [] { r, g, b, a }, 0);
		}
	}
}
