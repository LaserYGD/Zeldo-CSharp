using Engine;
using Engine.Physics;
using Engine.Sensors;
using Engine.Shapes._3D;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
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
			var size = Properties.GetFloat("door.sensor.size");

			// TODO: Limit the body size to exclude the handle.
			CreateBody(scene, new BoxShape(bounds.ToJVector()), RigidBodyTypes.PseudoStatic);
			CreateSensor(scene, new Box(size, bounds.y, bounds.z), SensorGroups.Interaction);

			base.Initialize(scene, data);
		}

		public void OnInteract(Entity entity)
		{
			// TODO: Open and transition the camera if appropriate.
		}
	}
}
