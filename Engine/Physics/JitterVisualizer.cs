using System;
using System.Collections.Generic;
using Engine.Core;
using Engine.Graphics._3D;
using Engine.Shapes._3D;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using Jitter;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;

namespace Engine.Physics
{
	public class JitterVisualizer
	{
		private World world;
		private PrimitiveRenderer3D primitives;

		public JitterVisualizer(Camera3D camera, World world)
		{
			this.world = world;

			primitives = new PrimitiveRenderer3D(camera, 10000, 1000);
		}

		public bool IsEnabled { get; set; }

		public void Draw(Camera3D camera)
		{
			if (!IsEnabled)
			{
				return;
			}

			foreach (RigidBody body in world.RigidBodies)
			{
				var shape = body.Shape;

				switch (shape.GetType().Name)
				{
					case "BoxShape": DrawBox((BoxShape)shape, body);
						break;

					case "CylinderShape": DrawCylinder((CylinderShape)shape, body);
						break;

					case "SphereShape": DrawSphere((SphereShape)shape, body);
						break;

					case "TriangleMeshShape": DrawTriangleMesh((TriangleMeshShape)shape, body);
						break;
				}
			}

			primitives.Flush();
		}

		private void DrawBox(BoxShape shape, RigidBody body)
		{
			var size = shape.Size;
			var box = new Box(size.X, size.Y, size.Z);
			box.Position = body.Position.ToVec3();
			box.Orientation = body.Orientation.ToQuat();

			primitives.Draw(box, Color.Cyan);
		}

		private void DrawCylinder(CylinderShape shape, RigidBody body)
		{
			const int Segments = 16;
			const float Increment = Constants.TwoPi / Segments;

			quat orientation = body.Orientation.ToQuat();
			vec3 center = body.Position.ToVec3();
			vec3 v = orientation * new vec3(0, shape.Height, 0);
			vec3 c0 = center - v / 2;

			float radius = shape.Radius;

			primitives.Draw(radius, c0, orientation, Color.Magenta, Segments);
			primitives.Draw(radius, center + v / 2, orientation, Color.Magenta, Segments);

			vec3[] points = new vec3[Segments];

			for (int i = 0; i < points.Length; i++)
			{
				vec2 p = Utilities.Direction(Increment * i) * radius;

				points[i] = orientation * new vec3(p.x, 0, p.y) + c0;
			}

			for (int i = 0; i < Segments; i++)
			{
				vec3 p = points[i];

				primitives.DrawLine(p, p + v, Color.Magenta);
			}
		}

		private void DrawSphere(SphereShape shape, RigidBody body)
		{
			const int Segments = 16;

			float radius = shape.Radius;

			vec3 center = body.Position.ToVec3();

			quat orientation = body.Orientation.ToQuat();
			quat q1 = orientation * quat.FromAxisAngle(Constants.PiOverTwo, vec3.UnitX);
			quat q2 = orientation * quat.FromAxisAngle(Constants.PiOverTwo, vec3.UnitZ);

			primitives.Draw(radius, center, orientation, Color.Green, Segments);
			primitives.Draw(radius, center, q1, Color.Green, Segments);
			primitives.Draw(radius, center, q2, Color.Green, Segments);
		}

		private void DrawTriangleMesh(TriangleMeshShape shape, RigidBody body)
		{
			var tuple = (Tuple<List<JVector>, List<TriangleVertexIndices>>)shape.Tag;
			var points = tuple.Item1;
			var tris = tuple.Item2;

			foreach (TriangleVertexIndices tri in tris)
			{
				vec3 p0 = points[tri.I0].ToVec3();
				vec3 p1 = points[tri.I1].ToVec3();
				vec3 p2 = points[tri.I2].ToVec3();

				primitives.DrawTriangle(p0, p1, p2, Color.White);
			}
		}
	}
}
