using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Items
{
	public class Bottle : Entity
	{
		public Bottle() : base(EntityGroups.Item)
		{
		}

		public override void Initialize(Scene scene, JToken data)
		{
			CreateModel(scene, "Bottle.obj");
		}
	}
}
