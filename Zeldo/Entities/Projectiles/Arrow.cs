using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Projectiles
{
	public class Arrow : Entity
	{
		public Arrow() : base(EntityGroups.Projectile)
		{
		}

		public override void Initialize(Scene scene, JToken data)
		{
			CreateModel(scene, "Arrow.obj");

			base.Initialize(scene, data);
		}
	}
}
