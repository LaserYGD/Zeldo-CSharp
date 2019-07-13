using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Enemies
{
	public class Groot : Enemy
	{
		public bool PoweredBySunlight { get; set; }

		public override void Initialize(Scene scene, JToken data)
		{
		}

		public override void Update(float dt)
		{
		}
	}
}
