using Engine.Shapes._3D;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Bosses.Tree
{
	public class RootFlower : LivingEntity
	{
		public RootFlower() : base(EntityGroups.Boss)
		{
		}

		public override void Initialize(Scene scene, JToken data)
		{
			var model = CreateModel(scene, "Bosses/RootFlower.obj");
			var bounds = model.Mesh.Bounds;

			CreateSensor(scene, new Sphere(bounds.x / 2), SensorGroups.Target);

			base.Initialize(scene, data);
		}

		protected override void OnDeath()
		{
		}
	}
}
