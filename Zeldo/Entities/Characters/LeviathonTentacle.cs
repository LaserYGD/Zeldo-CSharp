using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Characters
{
	public class LeviathonTentacle : Entity
	{
		public LeviathonTentacle() : base(EntityGroups.Character)
		{
		}

		public override void Initialize(Scene scene, JToken data)
		{
			var model = CreateModel(scene, "LeviathonTentacle.dae");

			base.Initialize(scene, data);
		}
	}
}
