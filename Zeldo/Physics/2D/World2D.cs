using System;
using System.Collections.Generic;
using Engine.Shapes._2D;
using Engine.Utility;
using GlmSharp;

namespace Zeldo.Physics._2D
{
	public class World2D
	{
		private List<RigidBody2D> staticBodies;
		private List<RigidBody2D> dynamicBodies;

		private float accumulator;

		public World2D()
		{
			staticBodies = new List<RigidBody2D>();
			dynamicBodies = new List<RigidBody2D>();
		}

		public List<RigidBody2D> StaticBodies => staticBodies;
		public List<RigidBody2D> DynamicBodies => dynamicBodies;

		public void Add(RigidBody2D body)
		{
			(body.IsStatic ? staticBodies : dynamicBodies).Add(body);
		}

		public void Remove(RigidBody2D body)
		{
		}

		public void Step(float dt, float step, int maxSteps)
		{
			accumulator += dt;

			int stepsTaken = 0;

			while (accumulator >= step && stepsTaken < maxSteps)
			{
				StepInternal(step);
				accumulator -= step;
				stepsTaken++;
			}
		}

		private void StepInternal(float step)
		{
			foreach (var dynamicBody in dynamicBodies)
			{
				if (!dynamicBody.IsEnabled)
				{
					continue;
				}

				dynamicBody.Position += dynamicBody.Velocity * step;

				List<vec2> correctionList = new List<vec2>();

				foreach (var staticBody in staticBodies)
				{
					if (!staticBody.IsEnabled)
					{
						continue;
					}

					if (ProcessBodies(dynamicBody, staticBody, out vec2 v))
					{
						correctionList.Add(v);
					}
				}

				if (correctionList.Count > 0)
				{
					dynamicBody.Position += MergeVectors(correctionList);
				}
			}
		}

		private bool ProcessBodies(RigidBody2D dynamicBody, RigidBody2D staticBody, out vec2 v)
		{
			v = vec2.Zero;
			
			float delta = Math.Abs(dynamicBody.Elevation - staticBody.Elevation);

			// Bodies capable of colliding should always be on the same plane. That said, comparing against a small,
			// fixed value (like one) feels safer than checking floating-point equality directly. What matters is that
			// the comparison value is smaller than the elevation difference between two bodies on different floors.
			if (delta > 1)
			{
				return false;
			}

			// This assumes that all character control shapes are circles.
			var dynamicCircle = (Circle)dynamicBody.Shape;
			var staticShape = staticBody.Shape;

			switch (staticShape.ShapeType)
			{
				case ShapeTypes2D.Circle:
					return ProcessCircle(dynamicCircle, (Circle)staticShape, ref v);

				case ShapeTypes2D.Line:
					return ProcessLine(dynamicCircle, (Line2D)staticShape, ref v);

				case ShapeTypes2D.Rectangle:
					return ProcessRectangle(dynamicCircle, (Rectangle)staticShape, ref v);
			}

			return false;
		}

		private bool ProcessCircle(Circle dynamicCircle, Circle staticCircle, ref vec2 v)
		{
			float r1 = dynamicCircle.Radius;
			float r2 = staticCircle.Radius;
			float sum = r1 + r2;
			float squared = Utilities.DistanceSquared(dynamicCircle.Position, staticCircle.Position);

			if (squared <= sum * sum)
			{
				float distance = (float)Math.Sqrt(squared);
				float penetration = r2 - distance;

				// This scenario shouldn't be possible in practice, but if two shapes are exactly overlapping, the
				// collision is resolved to the right.
				vec2 vector = distance == 0 ? vec2.UnitX : (dynamicCircle.Position - staticCircle.Position) / distance;
				v = vector * (penetration + r1);

				return true;
			}

			return false;
		}

		private bool ProcessLine(Circle dynamicCircle, Line2D staticLine, ref vec2 v)
		{
			float d = Utilities.DistanceSquaredToLine(dynamicCircle.Position, staticLine);
			float r = dynamicCircle.Radius;

			if (d < r * r)
			{
				float penetration = r - (float)Math.Sqrt(d);

				// This assumes that all static lines will be oriented such that the right-hand vector faces back into
				// the stage (in order to be used as correction vector).
				vec2 l = staticLine.P2 - staticLine.P1;
				vec2 outVector = new vec2(-l.y, l.x);

				v = Utilities.Normalize(outVector) * penetration;

				return true;
			}

			return false;
		}

		private bool ProcessRectangle(Circle dynamicCircle, Rectangle staticRect, ref vec2 v)
		{
			if (ProcessRectangleInternal(dynamicCircle, staticRect, ref v))
			{
				float rotation = staticRect.Rotation;

				if (rotation != 0)
				{
					v = Utilities.Rotate(v, rotation);
				}

				return true;
			}

			return false;
		}

		private bool ProcessRectangleInternal(Circle dynamicCircle, Rectangle staticRect, ref vec2 v)
		{
			vec2 p1 = dynamicCircle.Position;
			vec2 p2 = staticRect.Position;

			float rotation = staticRect.Rotation;

			if (rotation != 0)
			{
				p1 = Utilities.Rotate(p1 - p2, -rotation) + p2;
			}

			bool left = p1.x <= staticRect.Left;
			bool right = p1.x >= staticRect.Right;
			bool top = p1.y <= staticRect.Top;
			bool bottom = p1.y >= staticRect.Bottom;

			float radius = dynamicCircle.Radius;

			bool ProcessCorner(vec2 p, ref vec2 result)
			{
				float squared = Utilities.DistanceSquared(p1, p);

				if (squared <= radius * radius)
				{
					float distance = (float)Math.Sqrt(squared);

					result = (p1 - p) / distance * (radius - distance);

					return true;
				}

				return false;
			}

			bool ProcessHorizontal(ref vec2 result)
			{
				float dX = p1.x - p2.x;
				float abs = Math.Abs(dX);
				float halfWidth = staticRect.Width / 2;

				if (abs <= radius + halfWidth)
				{
					result.x = (radius + halfWidth - abs) * Math.Sign(dX);

					return true;
				}

				return false;
			}

			bool ProcessVertical(ref vec2 result)
			{
				float dY = p1.y - p2.y;
				float abs = Math.Abs(dY);
				float haflHeight = staticRect.Height / 2;

				if (abs <= radius + haflHeight)
				{
					result.y = (radius + haflHeight - abs) * Math.Sign(dY);

					return true;
				}

				return false;
			}

			// The logic here assumes that no dynamic circle will move fast enough over a single step for its center to
			// be contained within the rectangle.
			if (left)
			{
				if (top)
				{
					return ProcessCorner(new vec2(staticRect.Left, staticRect.Top), ref v);
				}

				return bottom
					? ProcessCorner(new vec2(staticRect.Left, staticRect.Bottom), ref v)
					: ProcessHorizontal(ref v);
			}

			if (right)
			{
				// Top-right corner.
				if (top)
				{
					return ProcessCorner(new vec2(staticRect.Right, staticRect.Top), ref v);
				}

				// Bottom-right corner.
				return bottom
					? ProcessCorner(new vec2(staticRect.Right, staticRect.Bottom), ref v)
					: ProcessHorizontal(ref v);
			}

			return ProcessVertical(ref v);
		}

		private vec2 MergeVectors(List<vec2> list)
		{
			int count = list.Count;

			if (count == 1)
			{
				return list[0];
			}

			vec2 final = vec2.Zero;

			for (int i = 0; i < count; i++)
			{
				vec2 v = list[i];
				final += v;

				for (int j = i + 1; j < count; j++)
				{
					list[i] = Utilities.Project(v, list[i]);
				}
			}

			return final;
		}
	}
}
