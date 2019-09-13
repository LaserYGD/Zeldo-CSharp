using System.Collections.Generic;
using Engine.Sensors;
using Engine.Shapes._2D;
using Engine.Timing;
using Engine.Utility;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Weapons
{
	public class Sword : MeleeWeapon
	{
		protected override void TriggerPrimary(out float cooldownTime, out float bufferTime)
		{
			cooldownTime = 0;
			bufferTime = 0;
		}
	}
}
