using System.Diagnostics;
using Engine;
using Engine.Core._3D;
using Engine.Physics;
using Engine.Physics.Verlet;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Physics;

namespace Zeldo.Entities.Windmill
{
	public class RopeBridge : Entity
	{
		private const float Damping = 0.95f;

		private VerletRope2D rope;
		private Model[] models;
		private RigidBody[] bodies;
		private quat flatOrientation;

		public RopeBridge() : base(EntityGroups.Structure)
		{
		}

		public override void Initialize(Scene scene, JToken data)
		{
			// TODO: Pull dimensions from the model.
			const float Width = 2.5f;
			const float Height = 0.35f;
			const float Depth = 11.37f;

			var p1 = Utilities.ParseVec3(data["P1"].Value<string>());
			var p2 = Utilities.ParseVec3(data["P2"].Value<string>());
			var count = data["SegmentCount"].Value<int>();
			var length = data["SegmentLength"].Value<float>();

			Debug.Assert(count > 0, "Segment count must be positive.");
			Debug.Assert(length > 0, "Segment length must be positive..");

			var points = new vec2[count + 1];

			// This value effectively stretches points beyond their target length, which results in a stiffer bridge.
			float scale = Utilities.Distance(p2, p1) / count / length;
			float yIncrement = (p2.y - p1.y) / count;

			for (int i = 0; i < points.Length; i++)
			{
				points[i] = new vec2(length * i * scale, -yIncrement * i);
			}

			position = p1;
			rope = new VerletRope2D(points, length, Damping, PhysicsConstants.Gravity);

			var angle = Utilities.Angle(p1.swizzle.xz, p2.swizzle.xz);
			flatOrientation = quat.FromAxisAngle(angle, vec3.UnitY);

			// Since rope endpoints are fixed, planks are only created for non-endpoints.
			bodies = new RigidBody[count - 1];
			models = new Model[bodies.Length];

			var shape = new BoxShape(Width, Height, Depth);
			var world = scene.World;

			for (int i = 0; i < bodies.Length; i++)
			{
				bodies[i] = CreateBody(scene, shape, RigidBodyTypes.PseudoStatic, RigidBodyFlags.None, false, false);

				var model = CreateModel(scene, "Cube.obj", false);
				model.Scale = new vec3(Width, Height, Depth);
				models[i] = model;
			}

			// Since rope bridges don't have a controlling body, the pre-step callback must be attached to the world
			// instead.
			world.Events.PreStep += PreStep;

			base.Initialize(scene, data);
		}

		private void PreStep(float step)
		{
			rope.Update(step);

			var points = rope.Points;

			for (int i = 1; i < points.Length - 1; i++)
			{
				var point = points[i];
				var raw = point.Position;
				var p = position + flatOrientation * new vec3(raw.x, -raw.y, 0);

				// I'm not certain why the rotation math works out this way, but it does.
				var orientation = flatOrientation * quat.FromAxisAngle(Constants.PiOverTwo * 3 - point.Rotation,
					vec3.UnitZ);

				bodies[i - 1].SetTransform(p.ToJVector(), orientation.ToJMatrix(), step);
				models[i - 1].SetTransform(p, orientation);
			}
		}

		public override void Dispose()
		{
			Scene.Renderer.Models.Remove(models);
			Scene.World.Remove(bodies);
		}
	}
}
