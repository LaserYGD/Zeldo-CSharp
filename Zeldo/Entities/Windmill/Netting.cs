using System.Diagnostics;
using Engine.Core;
using Engine.Physics;
using GlmSharp;
using Jitter.Dynamics;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Windmill
{
	// TODO: Generalize this class to generic cloth.
	public class Netting : Entity
	{
		private const int Size = 40;

		private SoftBody body;

		public Netting() : base(EntityGroups.Object)
		{
		}

		public override vec3 Position
		{
			get => base.Position;
			set
			{
				// TODO: Can cloth have position set after spawn?
				Debug.Assert(!isSpawnPositionSet, "Cloth position can only be set on spawn.");

				// Note that the base setter is intentionally not called (since soft bodies behave pretty differently
				// from rigid entities).
				position = value;
			}
		}

		public override quat Orientation
		{
			get => base.Orientation;
			set => Debug.Fail("Can't set orientation on cloth.");
		}

		public override void Dispose()
		{
			Scene.World.RemoveBody(body);
		}

		public override void Initialize(Scene scene, JToken data)
		{
			// TODO: Should position be passed in here?
			body = new SoftBody(Size, Size, 0.5f, position.ToJVector(), RigidBodyFlags.IsAffectedByGravity);
			//body.Translate(position.ToJVector());

			scene.World.AddBody(body);

			base.Initialize(scene, data);
		}

		public override void Update(float dt)
		{
			var primitives = Scene.Primitives;
			var points = body.VertexBodies;
			var color = Color.White;

			for (int i = 0; i < Size - 1; i++)
			{
				var start = i * Size;

				for (int j = 0; j < Size - 1; j++)
				{
					var p1 = points[start + j].Position.ToVec3();
					var p2 = points[start + j + 1].Position.ToVec3();
					var p3 = points[start + j + Size].Position.ToVec3();

					primitives.DrawLine(p1, p2, color);
					primitives.DrawLine(p1, p3, color);
				}
			}

			for (int i = 0; i < Size - 1; i++)
			{
				var p1 = points[(i + 1) * Size - 1].Position.ToVec3();
				var p2 = points[(i + 2) * Size - 1].Position.ToVec3();

				primitives.DrawLine(p1, p2, color);

				p1 = points[(Size - 1) * Size + i].Position.ToVec3();
				p2 = points[(Size - 1) * Size + i + 1].Position.ToVec3();

				primitives.DrawLine(p1, p2, color);
			}

			base.Update(dt);
		}
	}
}
