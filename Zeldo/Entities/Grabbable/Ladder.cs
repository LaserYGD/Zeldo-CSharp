using Engine.Utility;
using GlmSharp;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Grabbable
{
	public class Ladder : Entity, IInteractive, IGrabbable
	{
		public Ladder() : base(EntityGroups.Object)
		{
		}

		public GrabTypes GrabType => GrabTypes.Ladder;

		public bool IsInteractionEnabled => true;
		public bool RequiresFacing => true;

		public float Height { get; private set; }

		// Ladders can only be climbed from the front (defined by the ladder's rotation).
		public vec2 Facing { get; private set; }

		public override void Initialize(Scene scene, JToken data)
		{
			float rotation = data["Rotation"].Value<float>();
			int segments = data["Segments"].Value<int>();

			Facing = Utilities.Rotate(vec2.UnitX, rotation);

			base.Initialize(scene, data);
		}

		public void OnInteract(Entity entity)
		{
			((Player)entity).Mount(this);
		}
	}
}
