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
					case "BoxShape": Draw((BoxShape)shape, body);
						break;

					case "CapsuleShape": Draw((CapsuleShape)shape, body);
						break;

					case "CylinderShape": Draw((CylinderShape)shape, body);
						break;

					case "SphereShape": Draw((SphereShape)shape, body);
						break;

					case "TriangleMeshShape": Draw((TriangleMeshShape)shape, body);
						break;
				}
			}

			primitives.Flush();
		}

		private void Draw(BoxShape shape, RigidBody body)
		{
			var size = shape.Size;
			var box = new Box(size.X, size.Y, size.Z);
			box.Position = body.Position.ToVec3();
			box.Orientation = body.Orientation.ToQuat();

			Color color = Color.White;

			switch (body.BodyType)
			{
				case RigidBodyTypes.Dynamic: color = Color.Red;
					break;

				case RigidBodyTypes.Kinematic: color = Color.Green;
					break;

				case RigidBodyTypes.Static: color = Color.Yellow;
					break;
			}

			primitives.Draw(box, color);
		}

		private void Draw(CapsuleShape shape, RigidBody body)
		{
			const int Segments = 16;
			const int Rings = 5;

			float l = shape.Length / 2;
			float r = shape.Radius;

			quat orientation = body.Orientation.ToQuat();
			vec3 p = body.Position.ToVec3();
			vec3 v = vec3.UnitY * orientation;
			Color color = Color.Green;

			for (int i = 0; i < Rings; i++)
			{
				float radius = (float)Math.Cos(Constants.PiOverTwo / Rings * i) * r;
				float offset = r / Rings * i;

				primitives.DrawCircle(radius, p + v * (l + offset), orientation, color, Segments);
				primitives.DrawCircle(radius, p - v * (l + offset), orientation, color, Segments);
			}

			for (int i = 0; i < Segments; i++)
			{
				vec2 d = Utilities.Direction(Constants.TwoPi / Segments * i) * r;
				vec3 point = new vec3(d.x, 0, d.y) * orientation + p;

				primitives.DrawLine(point + v * l, point - v * l, color);
			}
		}

		private void Draw(CylinderShape shape, RigidBody body)
		{
			const int Segments = 16;
			const float Increment = Constants.TwoPi / Segments;

			quat orientation = body.Orientation.ToQuat();
			vec3 center = body.Position.ToVec3();
			vec3 v = orientation * new vec3(0, shape.Height, 0);
			vec3 c0 = center - v / 2;

			float radius = shape.Radius;

			primitives.DrawCircle(radius, c0, orientation, Color.Magenta, Segments);
			primitives.DrawCircle(radius, center + v / 2, orientation, Color.Magenta, Segments);

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

		private void Draw(SphereShape shape, RigidBody body)
		{
			const int Segments = 16;

			float radius = shape.Radius;

			vec3 center = body.Position.ToVec3();

			quat orientation = body.Orientation.ToQuat();
			quat q1 = orientation * quat.FromAxisAngle(Constants.PiOverTwo, vec3.UnitX);
			quat q2 = orientation * quat.FromAxisAngle(Constants.PiOverTwo, vec3.UnitZ);

			primitives.DrawCircle(radius, center, orientation, Color.Green, Segments);
			primitives.DrawCircle(radius, center, q1, Color.Green, Segments);
			primitives.DrawCircle(radius, center, q2, Color.Green, Segments);
		}

		private void Draw(TriangleMeshShape shape, RigidBody body)
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
