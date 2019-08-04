using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Interfaces._2D;
using Engine.Shapes._2D;
using GlmSharp;

namespace Engine.Utility
{
	public static class Utilities
	{
		public static int EnumCount<T>()
		{
			return Enum.GetValues(typeof(T)).Length;
		}

		public static T EnumParse<T>(string value)
		{
			return (T)Enum.Parse(typeof(T), value);
		}

		public static byte Lerp(byte start, byte end, float t)
		{
			return (byte)((end - start) * t + start);
		}

		public static int Lerp(int start, int end, float t)
		{
			return (int)((end - start) * t) + start;
		}

		public static float Lerp(float start, float end, float t)
		{
			return (end - start) * t + start;
		}

		public static float Length(vec2 v)
		{
			return (float)Math.Sqrt(LengthSquared(v));
		}

		public static float Length(vec3 v)
		{
			return (float)Math.Sqrt(LengthSquared(v));
		}

		public static float LengthSquared(vec2 v)
		{
			return v.x * v.x + v.y * v.y;
		}

		public static float LengthSquared(vec3 v)
		{
			return v.x * v.x + v.y * v.y + v.z * v.z;
		}

		public static float Distance(vec2 p1, vec2 p2)
		{
			return Length(p2 - p1);
		}

		public static float Distance(vec3 p1, vec3 p2)
		{
			return Length(p2 - p1);
		}

		public static float DistanceSquared(vec2 p1, vec2 p2)
		{
			return LengthSquared(p2 - p1);
		}

		public static float DistanceSquared(vec3 p1, vec3 p2)
		{
			return LengthSquared(p2 - p1);
		}

		public static float DistanceToLine(vec2 p, Line2D line)
		{
			return DistanceToLine(p, line.P1, line.P2);
		}

		public static float DistanceToLine(vec2 p, vec2 l1, vec2 l2)
		{
			return (float)Math.Sqrt(DistanceSquaredToLine(p, l1, l2));
		}

		public static float DistanceSquaredToLine(vec2 p, Line2D line)
		{
			return DistanceSquaredToLine(p, line.P1, line.P2);
		}

		public static float DistanceSquaredToLine(vec2 p, vec2 l1, vec2 l2)
		{
			// See https://stackoverflow.com/a/1501725/7281613. It's assumed that the two line endpoints won't be the
			// same, but even if they are, I think the code should be okay.
			float squared = DistanceSquared(l1, l2);
			float t = Math.Max(0, Math.Min(1, vec2.Dot(p - l1, l2 - l1) / squared));

			vec2 projection = l1 + t * (l2 - l1);

			return DistanceSquared(p, projection);
		}

		public static float Angle(vec2 p1, vec2 p2)
		{
			return Angle(p2 - p1);
		}

		public static float Angle(vec2 v)
		{
			return (float)Math.Atan2(v.y, v.x);
		}

		public static float CorrectAngle(float angle)
		{
			if (angle > Constants.Pi)
			{
				angle = Constants.TwoPi - angle;
			}

			return angle;
		}

		public static float Clamp(float v, float min, float max)
		{
			// Min and max are inclusive.
			if (v < min)
			{
				return min;
			}

			return v > max ? max : v;
		}

		public static float Dot(vec2 v1, vec2 v2)
		{
			return vec2.Dot(v1, v2);
		}

		public static float Dot(vec3 v1, vec3 v2)
		{
			return vec3.Dot(v1, v2);
		}

		public static vec2 Direction(float angle)
		{
			float x = (float)Math.Cos(angle);
			float y = (float)Math.Sin(angle);

			return new vec2(x, y);
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

		public static vec2 ParseVec2(string value)
		{
			// The assumed format is "X|Y".
			string[] tokens = value.Split('|');

			float x = float.Parse(tokens[0]);
			float y = float.Parse(tokens[1]);

			return new vec2(x, y);
		}

		public static vec3 ParseVec3(string value)
		{
			// The assumed format is "X|Y|Z".
			string[] tokens = value.Split('|');

			float x = float.Parse(tokens[0]);
			float y = float.Parse(tokens[1]);
			float z = float.Parse(tokens[2]);

			return new vec3(x, y, z);
		}

		public static vec2 Project(vec2 v, vec2 onto)
		{
			// See https://math.oregonstate.edu/home/programs/undergrad/CalculusQuestStudyGuides/vcalc/dotprod/dotprod.html.
			return vec2.Dot(onto, v) / vec2.Dot(onto, onto) * onto;
		}

		public static vec2 Normalize(vec2 v)
		{
			if (v == vec2.Zero)
			{
				return vec2.Zero;
			}

			return v / Length(v);
		}

		public static vec3 Normalize(vec3 v)
		{
			if (v == vec3.Zero)
			{
				return vec3.Zero;
			}

			return v / Length(v);
		}

		public static vec2 Rotate(vec2 v, float rotation)
		{
			return rotation == 0 ? v : RotationMatrix2D(rotation) * v;
		}

		public static mat2 RotationMatrix2D(float rotation)
		{
			float sin = (float)Math.Sin(rotation);
			float cos = (float)Math.Cos(rotation);

			return new mat2(cos, sin, -sin, cos);
		}

		public static vec3 Reflect(vec3 v, vec3 normal)
		{
			// See https://math.stackexchange.com/a/13263.
			return v - 2 * vec3.Dot(v, normal) * normal;
		}

		public static vec3 Cross(vec3 v1, vec3 v2)
		{
			return vec3.Cross(v1, v2);
		}

		public static void PositionItems(IPositionable2D[] items, vec2 start, vec2 spacing)
		{
			for (int i = 0; i < items.Length; i++)
			{
				items[i].Position = start + spacing * i;
			}
		}

		public static T Closest<T>(IEnumerable<T> items, vec2 position) where T : class, IPositionable2D
		{
			T closest = null;

			float d = float.MaxValue;

			foreach (T item in items)
			{
				float squared = Utilities.DistanceSquared(position, item.Position);

				if (squared < d)
				{
					d = squared;
					closest = item;
				}
			}

			return closest;
		}

		public static string[] WrapLines(string value, SpriteFont font, int width)
		{
			List<string> lines = new List<string>();
			string[] words = value.Split(' ');
			StringBuilder builder = new StringBuilder();

			int currentWidth = 0;
			int spaceWidth = font.Measure(" ").x;

			foreach (string word in words)
			{
				int wordWidth = font.Measure(word).x;

				if (currentWidth + wordWidth > width)
				{
					lines.Add(builder.ToString());
					builder.Clear();
					builder.Append(word + " ");
					currentWidth = wordWidth + spaceWidth;
				}
				else
				{
					currentWidth += wordWidth + spaceWidth;
					builder.Append(word + " ");
				}
			}

			if (builder.Length > 0)
			{
				lines.Add(builder.ToString());
			}

			return lines.ToArray();
		}
	}
}
