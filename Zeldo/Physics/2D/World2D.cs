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
				dynamicBody.Position += dynamicBody.Velocity * step;

				List<vec2> correctionList = new List<vec2>();

				foreach (var staticBody in staticBodies)
				{
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

				case ShapeTypes2D.Rectangle:
					return ProcessLine(dynamicCircle, (Rectangle)staticShape, ref v);
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

		private bool ProcessLine(Circle dynamicCircle, Rectangle staticRect, ref vec2 v)
		{
			return false;
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
