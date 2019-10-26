using System;
using System.Diagnostics;
using Engine;
using Engine.Core;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Entities.Player;
using Zeldo.Interfaces;
using Zeldo.UI;

namespace Zeldo.Entities
{
	// TODO: Should the ladder be grabbable? (if so, should extend IGrabbable)
	public class Ladder : Entity, IAscendable
	{
		private static readonly float SideSlice = Properties.GetFloat("ladder.side.slice");

		// Ladders can be climbed from any angle, but the player whips around to the front when grabbing from the side
		// or back.
		private float flatRotation;

		public Ladder() : base(EntityGroups.Object)
		{
		}

		// This is used by the ladder controller.
		public float Length { get; private set; }
		public float CosineTilt { get; private set; }

		public override void Initialize(Scene scene, JToken data)
		{
			flatRotation = data["Rotation"].Value<float>();

			// Ladders can optionally be tilted slightly forward.
			var orientation = quat.FromAxisAngle(flatRotation, vec3.UnitY);

			if (data.TryGetValue("Tilt", out float tilt))
			{
				CosineTilt = (float)Math.Cos(tilt);
				orientation *= quat.FromAxisAngle(tilt, vec3.UnitZ);
			}

			Orientation = orientation;

			var top = data["Top"].Value<float>();
			var bottom = data["Bottom"].Value<float>();
			var delta = top - bottom;

			Debug.Assert(top > bottom, "Ladder top must be greater than bottom.");

			// If tilt is specified, length is automatically computed to match the given top and bottom.
			Length = tilt != 0
				? delta / CosineTilt
				: delta;

			// Ladder position corresponds to its bottom-center point.
			position += orientation * vec3.UnitY * Length / 2;

			// TODO: Set dimensions based on mesh bounds.
			CreateBody(scene, new BoxShape(0.1f, Length, 1), RigidBodyTypes.PseudoStatic);

			base.Initialize(scene, data);
		}

		// TODO: Generalize to touching the ladder from the ground as well.
		// By the time this function is called, the player is guaranteed already 1) touching the ladder while airborne,
		// and 2) facing the ladder. This function verifies that the player is also in front
		public Proximities ComputeProximity(vec3 p)
		{
			return Utilities.ComputeProximity(position, p, flatRotation, SideSlice);
		}

		public vec3 ComputeAscension(float t)
		{
			return vec3.Zero;
		}

		public override void Update(float dt)
		{
			var d = Utilities.Direction(flatRotation);

			Scene.DebugPrimitives.DrawLine(Position, Position + new vec3(d.x, 0, d.y), Color.Cyan);

			base.Update(dt);
		}
	}
}
