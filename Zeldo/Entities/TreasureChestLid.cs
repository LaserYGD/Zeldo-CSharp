using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Smoothers._3D;
using Engine.Utility;
using GlmSharp;
using Zeldo.Entities.Core;

namespace Zeldo.Entities
{
	public class TreasureChestLid : Entity
	{
		public TreasureChestLid() : base(EntityTypes.Object)
		{
		}

		public void Open()
		{
			quat start = quat.Identity;
			quat end = quat.FromAxisAngle(90, -vec3.UnitZ);

			Components.Add(new OrientationSmoother(this, start, end, 0.5f, EaseTypes.Linear));
		}
	}
}
