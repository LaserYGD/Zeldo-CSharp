using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Characters
{
	public class LeviathonHead : Entity
	{
		public LeviathonHead() : base(EntityGroups.Character)
		{
		}

		public override void Initialize(Scene scene, JToken data)
		{
			var model = CreateModel(scene, "LeviathonHead.dae");

			base.Initialize(scene, data);
		}
	}
}
