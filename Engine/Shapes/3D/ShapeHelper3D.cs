using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Utility;

namespace Engine.Shapes._3D
{
	// TODO: Finish all implementations.
	public static class ShapeHelper3D
	{
		public static bool Overlaps(Shape3D shape1, Shape3D shape2)
		{
			ShapeTypes3D type1 = shape1.ShapeType;
			ShapeTypes3D type2 = shape2.ShapeType;

			bool isPoint1 = type1 == ShapeTypes3D.Point;
			bool isPoint2 = type2 == ShapeTypes3D.Point;

			if (isPoint1)
			{
				var p1 = shape1.Position;

				return isPoint2 ? p1 == shape2.Position : shape2.Contains(p1);
			}

			if (isPoint2)
			{
				return shape1.Contains(shape2.Position);
			}

			if ((int)type1 > (int)type2)
			{
				Shape3D temp = shape1;
				shape1 = shape2;
				shape2 = temp;
			}

			switch (type1)
			{
				case ShapeTypes3D.Box: return Overlaps((Box)shape1, shape2);
				case ShapeTypes3D.Line: return Overlaps((Line3D)shape1, shape2);
				case ShapeTypes3D.Sphere: return Overlaps((Sphere)shape1, (Sphere)shape2);
			}

			return false;
		}

		private static bool Overlaps(Box box, Shape3D other)
		{
			switch (other.ShapeType)
			{
				case ShapeTypes3D.Box: return Overlaps(box, (Box)other);
				case ShapeTypes3D.Line: return Overlaps(box, (Line3D)other);
				case ShapeTypes3D.Sphere: return Overlaps(box, (Sphere)other);
			}

			return false;
		}

		private static bool Overlaps(Box box1, Box box2)
		{
			return false;
		}

		private static bool Overlaps(Box box, Line3D line)
		{
			return false;
		}

		private static bool Overlaps(Box box, Sphere sphere)
		{
			return false;
		}

		private static bool Overlaps(Line3D line, Shape3D other)
		{
			switch (other.ShapeType)
			{
				case ShapeTypes3D.Line: break;
				case ShapeTypes3D.Sphere: break;
			}

			return false;
		}

		private static bool Overlaps(Line3D line1, Line3D line2)
		{
			return false;
		}

		private static bool Overlaps(Line3D line, Sphere sphere)
		{
			return false;
		}

		private static bool Overlaps(Sphere sphere1, Sphere sphere2)
		{
			float squared = Utilities.DistanceSquared(sphere1.Position, sphere2.Position);
			float sum = sphere1.Radius + sphere2.Radius;

			return squared <= sum * sum;
		}
	}
}
