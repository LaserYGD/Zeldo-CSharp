using Engine.Physics;
using Jitter.Collision.Shapes;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Objects
{
	public class Door : Entity, IInteractive
	{
		private bool isLocked;

		public Door() : base(EntityGroups.Object)
		{
		}

		public bool IsInteractionEnabled => true;
		public bool RequiresFacing => true;

		public override void Initialize(Scene scene, JToken data)
		{
			var bounds = CreateModel(scene, "Door.obj").Mesh.Bounds;

			CreateBody(scene, new BoxShape(bounds.ToJVector()));

			base.Initialize(scene, data);
		}

		public void OnInteract(Entity entity)
		{
			// TODO: Open and transition the camera if appropriate.
		}
	}
}
