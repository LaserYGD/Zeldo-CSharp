﻿using Engine;
using Engine.Core;

namespace Zeldo.Entities.Windmill
{
	public class MotorTree : Component
	{
		private float rotation;

		public MotorNode[] Nodes { get; set; }

		public float AngularVelocity { get; set; }

		public override void Update(float dt)
		{
			rotation += AngularVelocity * dt;

			// This always keeps the net rotation between zero and two pi.
			if (rotation > Constants.TwoPi)
			{
				rotation -= Constants.TwoPi;
			}
			else if (rotation < 0)
			{
				rotation += Constants.TwoPi;
			}

			// This assumes that the first entry is the root node.
			Nodes[0].Apply(rotation);
		}
	}
}