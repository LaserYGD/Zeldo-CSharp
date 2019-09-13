using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeldo.Entities.Weapons
{
	public class Twinblade : MeleeWeapon
	{
		protected override void TriggerPrimary(out float cooldownTime, out float bufferTime)
		{
			cooldownTime = 0.75f;
			bufferTime = 0.4f;
		}
	}
}
