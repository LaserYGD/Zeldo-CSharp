using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;

namespace Zeldo.Interfaces
{
	public interface ITargetable
	{
		void OnHit(int damage, int knockback, float angle, vec2 direction, object source);
	}
}
