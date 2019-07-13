using Engine;
using Engine.Shapes._2D;
using Jitter.Collision.Shapes;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Objects
{
	public class Cannonball : Entity, IInteractive
	{
		public Cannonball() : base(EntityGroups.Object)
		{
		}

		public bool IsInteractionEnabled => true;

		public override void Initialize(Scene scene, JToken data)
		{
			float collisionRadius = Properties.GetFloat("cannonball.collision.radius");
			float pickupRadius = Properties.GetFloat("cannonball.pickup.radius");

			CreateModel(scene, "Cannonball.obj");
			CreateSensor(scene, new Circle(pickupRadius));
			CreateRigidBody3D(scene, new SphereShape(collisionRadius));

			base.Initialize(scene, data);
		}

		public void OnInteract(Entity entity)
		{
		}
	}
}
