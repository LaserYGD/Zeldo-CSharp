using Engine.Physics;
using Engine.Shapes._2D;
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

		public override void Initialize(Scene scene, JToken data)
		{
			var bounds = CreateModel(scene, "Door.obj").Mesh.Bounds;

			CreateGroundBody(scene, new Rectangle(bounds.x, bounds.z), true);
			CreateRigidBody3D(scene, new BoxShape(bounds.ToJVector()));

			base.Initialize(scene, data);
		}

		public void OnInteract(Entity entity)
		{
			// Open and transition the camera if appropriate
		}
	}
}
