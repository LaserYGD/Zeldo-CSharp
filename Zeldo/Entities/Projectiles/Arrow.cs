using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Entities;
using GlmSharp;
using Zeldo.Physics;

namespace Zeldo.Entities.Projectiles
{
	public class Arrow : Entity3D
	{
		public VerletPoint RopeEndpoint { get; set; }

		public override vec3 Position
		{
			get => base.Position;
			set
			{
				if (RopeEndpoint != null)
				{
					RopeEndpoint.Position = value;
				}

				base.Position = value;
			}
		}
	}
}
