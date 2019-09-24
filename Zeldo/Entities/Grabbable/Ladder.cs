using System;
using Engine;
using Engine.Core;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;
using Zeldo.UI;

namespace Zeldo.Entities.Grabbable
{
	// TODO: Should the ladder be classified as grabbable? (if not, it'll have to be moved to a different namespace)
	public class Ladder : Entity, IInteractive, IGrabbable
	{
		private static readonly float HalfSideSlice = Properties.GetFloat("ladder.side.slice") / 2;

		// Ladders can be climbed from any angle, but the player whips around to the front when grabbing from the side
		// or back.
		private float facing;

		public Ladder() : base(EntityGroups.Object)
		{
		}

		public GrabTypes GrabType => GrabTypes.Ladder;

		public bool IsInteractionEnabled => true;
		public bool RequiresFacing => true;

		public float Height { get; private set; }

		public override void Initialize(Scene scene, JToken data)
		{
			// TODO: Load ladders from a file.
			//float rotation = data["Rotation"].Value<float>();
			//int segments = data["Segments"].Value<int>();

			//Facing = Utilities.Rotate(vec2.UnitX, rotation);
			facing = 0;

			// TODO: Compute dimensions based on mesh bounds and number of segments.
			CreateBody(scene, new BoxShape(0.1f, 20, 1), RigidBodyTypes.Static, false);

			base.Initialize(scene, data);
		}

		// By the time this function is called, the player is guaranteed already 1) touching the ladder while airborne,
		// and 2) facing the ladder. This function verifies that the player is also in front
		public LadderZones GetZone(vec3 p)
		{
			float angle = Utilities.Angle(p.swizzle.xz, position.swizzle.xz);
			float delta = Math.Abs(facing - angle);

			if (delta > Constants.Pi)
			{
				delta = Constants.TwoPi - delta;
			}

			delta = Constants.PiOverTwo - delta;

			var debug = Scene.Canvas.GetElement<DebugView>();
			debug.Add("Ladder", $"Angle: {angle:N2}");
			debug.Add("Ladder", $"Delta: {delta:N2}");

			if (Math.Abs(delta) < HalfSideSlice)
			{
				return LadderZones.Side;
			}

			return delta > 0 ? LadderZones.Front : LadderZones.Back;
		}

		public void OnInteract(Entity entity)
		{
			((Player)entity).Mount(this);
		}

		public override void Update(float dt)
		{
			var player = Scene.GetEntities<Player>(EntityGroups.Player)[0];
			var d = Utilities.Direction(facing);

			Scene.DebugPrimitives.DrawLine(Position, Position + new vec3(d.x, 0, d.y), Color.Cyan);
			Scene.Canvas.GetElement<DebugView>().Add("Ladder", "Zone: " + GetZone(player.Position));

			base.Update(dt);
		}
	}
}
