using System.Linq;
using Engine.Physics;
using Engine.Sensors;
using Engine.Utility;
using GlmSharp;
using Jitter;
using Jitter.Collision;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Zeldo.Entities.Core;

namespace Zeldo.State
{
	public class GameplayLoop : GameLoop
	{
		public const float PhysicsStep = 1f / 120;

		// TODO: Use properties.
		public const int PhysicsIterations = 8;
		public const int Gravity = -18;

		private World world;
		private Scene scene;
		private Space space;

		public override void Initialize()
		{
			CollisionSystem system = new CollisionSystemSAP();
			system.UseTriangleMeshNormal = true;
			system.CollisionDetected += OnCollision;

			// TODO: Should damping factors be left in their default states? (they were changed while adding kinematic bodies)
			world = new World(system);
			world.Gravity = new JVector(0, Gravity, 0);
			world.SetDampingFactors(1, 1);

			space = new Space();
			scene = new Scene
			{
				Camera = camera,
				Canvas = canvas,
				Space = space,
				World = world
			};

			scene.LoadFragment("Triangle.json");
		}

		private void OnCollision(RigidBody body1, RigidBody body2, JVector point1, JVector point2, JVector normal,
			JVector[] triangle, float penetration)
		{
			// By design, all physics objects have entities attached, with the exception of static parts of the
			// map. In the case of map collisions, it's unknown which body comes first as an argument (as of
			// writing this comment, anyway), which is why both entities are checked for null.
			Entity entity1 = body1.Tag as Entity;
			Entity entity2 = body2.Tag as Entity;

			vec3 p1 = point1.ToVec3();
			vec3 p2 = point2.ToVec3();

			// The normal needs to be flipped based on how Jitter handles triangle winding.
			vec3 n = Utilities.Normalize(-normal.ToVec3());

			// A triangle will only be given in the case of collisions with the map.
			if (triangle != null)
			{
				var entity = entity1 ?? entity2;
				var point = entity1 != null ? p2 : p1;
				var tArray = triangle.Select(t => t.ToVec3()).ToArray();

				entity.OnCollision(point, n, tArray);

				return;
			}

			entity1?.OnCollision(entity2, p1, -n, penetration);
			entity2?.OnCollision(entity1, p2, n, penetration);
		}

		public override void Dispose()
		{
		}

		public override void Update(float dt)
		{
			world.Step(dt, true, PhysicsStep, PhysicsIterations);
			space.Update();
			scene.Update(dt);
		}

		public override void Draw()
		{
		}
	}
}
