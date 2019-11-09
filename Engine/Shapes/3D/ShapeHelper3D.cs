using System;
using System.Linq;
using Engine.Shapes._2D;
using Engine.Utility;
using GlmSharp;

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
				var temp1 = shape1;
				shape1 = shape2;
				shape2 = temp1;

				type1 = type2;
			}

			switch (type1)
			{
				case ShapeTypes3D.Box: return Overlaps((Box)shape1, shape2);
				case ShapeTypes3D.Cylinder: return Overlaps((Cylinder)shape1, shape2);
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
				case ShapeTypes3D.Cylinder: return Overlaps(box, (Cylinder)other);
				case ShapeTypes3D.Line: return Overlaps(box, (Line3D)other);
				case ShapeTypes3D.Sphere: return Overlaps(box, (Sphere)other);
			}

			return false;
		}

		private static bool Overlaps(Box box1, Box box2)
		{
			return false;
		}

		private static bool Overlaps(Box box, Cylinder cylinder)
		{
			bool isOrientable1 = box.IsOrientable;
			bool isFixedVertical1 = box.IsFixedVertical;
			bool isOrientable2 = cylinder.IsOrientable;

			var p1 = box.Position;
			var p2 = cylinder.Position;

			// The cylinder is non-orientable (while the box is either non-orientable or fixed-vertical).
			if (!isOrientable2 && (isFixedVertical1 || !isOrientable1))
			{
				// Check Y delta.
				var dY = Math.Abs(p1.y - p2.y);

				if (dY > (box.Height + cylinder.Height) / 2)
				{
					return false;
				}

				// The calculations below are the same for fixed-vertical boxes, except that the boxes flat vertices
				// need to be rotated relative to the cylinder.
				if (isFixedVertical1)
				{
					p1 = p2 + box.Orientation.Inverse * (p1 - p2);
				}

				// Check cylinder zone (compared to the box).
				var dX = Math.Abs(p1.x - p2.x);
				var dZ = Math.Abs(p1.z - p2.z);
				var halfBounds = box.Bounds / 2;
				var withinX = dX <= halfBounds.x;
				var withinZ = dZ <= halfBounds.z;

				// This means that the cylinder's central axis is within the box (along the flat XZ plane).
				if (withinX && withinZ)
				{
					return true;
				}

				var radius = cylinder.Radius;

				// Check X delta.
				if (withinX)
				{
					return dZ <= halfBounds.z + radius;
				}

				// Check Z delta.
				if (withinZ)
				{
					return dX <= halfBounds.x + radius;
				}

				var flatP1 = p1.swizzle.xz;
				var flatP2 = p2.swizzle.xz;
				var flatCorners = new []
				{
					new vec2(halfBounds.x, halfBounds.z),
					new vec2(halfBounds.x, -halfBounds.z),
					new vec2(-halfBounds.x, halfBounds.z),
					new vec2(-halfBounds.x, -halfBounds.z)
				};

				return flatCorners.Any(p => Utilities.DistanceSquared(flatP1 + p, flatP2) <= radius * radius);
			}

			// TODO: Finish this (for orientable boxes/cylinders).
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

		private static bool Overlaps(Cylinder cylinder, Shape3D other)
		{
			switch (other.ShapeType)
			{
				case ShapeTypes3D.Cylinder: return Overlaps(cylinder, (Cylinder)other);
				case ShapeTypes3D.Line: return Overlaps(cylinder, (Line3D)other);
				case ShapeTypes3D.Sphere: return Overlaps(cylinder, (Sphere)other);
			}

			return false;
		}

		private static bool Overlaps(Cylinder cylinder1, Cylinder cylinder2)
		{
			bool isOrientable1 = cylinder1.IsOrientable;
			bool isOrientable2 = cylinder2.IsOrientable;

			// Both shapes are non-orientable.
			if (!isOrientable1 && !isOrientable2)
			{
				var p1 = cylinder1.Position;
				var p2 = cylinder2.Position;

				float delta = Math.Abs(p1.y - p2.y);
				float sum = (cylinder1.Height + cylinder2.Height) / 2;

				if (delta > sum)
				{
					return false;
				}

				float sumRadii = cylinder1.Radius + cylinder2.Radius;

				return Utilities.DistanceSquared(p1.swizzle.xz, p2.swizzle.xz) <= sumRadii * sumRadii;
			}

			// TODO: Finish this (for orientable cylinders).
			return false;
		}

		private static bool Overlaps(Cylinder cylinder, Line3D line)
		{
			return false;
		}

		private static bool Overlaps(Cylinder cylinder, Sphere sphere)
		{
			return false;
		}

		private static bool Overlaps(Line3D line, Shape3D other)
		{
			switch (other.ShapeType)
			{
				case ShapeTypes3D.Line: return Overlaps(line, (Line3D)other);
				case ShapeTypes3D.Sphere: return Overlaps(line, (Sphere)other);
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
