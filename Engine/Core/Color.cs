using System;
using Engine.Utility;
using GlmSharp;

namespace Engine.Core
{
	public struct Color
	{
		public static readonly Color Black = new Color(0);
		public static readonly Color White = new Color(255);
		public static readonly Color Red = new Color(255, 0, 0);
		public static readonly Color Green = new Color(0, 255, 0);
		public static readonly Color Blue = new Color(0, 0, 255);
		public static readonly Color Yellow = new Color(255, 255, 0);
		public static readonly Color Cyan = new Color(0, 255, 255);
		public static readonly Color Magenta = new Color(255, 0, 255);
		public static readonly Color Transparent = new Color(0, 0, 0, 0);

		public static Color Lerp(Color start, Color end, float t)
		{
			byte r = Utilities.Lerp(start.R, end.R, t);
			byte g = Utilities.Lerp(start.G, end.G, t);
			byte b = Utilities.Lerp(start.B, end.B, t);
			byte a = Utilities.Lerp(start.A, end.A, t);

			return new Color(r, g, b, a);
		}

		public Color(byte value) : this(value, value, value, 255)
		{
		}

		public Color(byte r, byte g, byte b) : this(r, g, b, 255)
		{
		}

		public Color(byte r, byte g, byte b, byte a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public byte R { get; set; }
		public byte G { get; set; }
		public byte B { get; set; }
		public byte A { get; set; }

		public float ToFloat()
		{
			return BitConverter.ToSingle(new [] { R, G, B, A }, 0);
		}

		public vec3 ToVec3()
		{
			return new vec3(R, G, B) / 255;
		}

		public vec4 ToVec4()
		{
			return new vec4(R, G, B, A) / 255;
		}
	}
}
