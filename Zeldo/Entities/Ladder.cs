using System;
using Engine;
using Engine.Core;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Entities.Player;
using Zeldo.Interfaces;
using Zeldo.UI;

namespace Zeldo.Entities
{
	// TODO: Should the ladder be grabbable? (if so, should extend IGrabbable)
	public class Ladder : Entity, IInteractive, IAscendable
	{
		private static readonly float SideSlice = Properties.GetFloat("ladder.side.slice");

		// Ladders can be climbed from any angle, but the player whips around to the front when grabbing from the side
		// or back.
		private float facing;

		public Ladder() : base(EntityGroups.Object)
		{
		}

		public bool IsInteractionEnabled => true;
		public bool RequiresFacing => true;

		// TODO: Fill in these ascension values.
		public float AscensionTop { get; }
		public float AscensionBottom { get; }
		public float Height { get; private set; }

		// While climbing a ladder, actor position is set based on direction and a progress value.
		public vec2 FacingDirection { get; private set; }
		public vec2 AscensionAxis => position.swizzle.xz;

		public override void Initialize(Scene scene, JToken data)
		{
			// TODO: Load ladders from a file.
			//float rotation = data["Rotation"].Value<float>();
			//int segments = data["Segments"].Value<int>();

			facing = Constants.Pi;
			FacingDirection = Utilities.Direction(facing);

			// TODO: Compute dimensions based on mesh bounds and number of segments.
			CreateBody(scene, new BoxShape(0.1f, 15, 1), RigidBodyTypes.Static, false);

			base.Initialize(scene, data);
		}

		// By the time this function is called, the player is guaranteed already 1) touching the ladder while airborne,
		// and 2) facing the ladder. This function verifies that the player is also in front
		public Proximities ComputeProximity(vec3 p)
		{
			return Utilities.ComputeProximity(position, p, facing, SideSlice);
		}

		public void OnInteract(Entity entity)
		{
			((PlayerCharacter)entity).Mount(this);
		}

		public override void Update(float dt)
		{
			var d = Utilities.Direction(facing);

			Scene.DebugPrimitives.DrawLine(Position, Position + new vec3(d.x, 0, d.y), Color.Cyan);

			base.Update(dt);
		}
	}
}
