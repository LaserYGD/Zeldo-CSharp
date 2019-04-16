using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Projectiles
{
	public class Arrow : Entity
	{
		public Arrow() : base(EntityTypes.Projectile)
		{
		}
	}
}
