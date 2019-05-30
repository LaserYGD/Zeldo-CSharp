using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Enemies
{
	public abstract class Enemy : LivingEntity
	{
		protected Enemy() : base(EntityGroups.Enemy)
		{
		}
	}
}
