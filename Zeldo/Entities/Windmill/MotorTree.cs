using System.Diagnostics;
using Engine;
using Engine.Interfaces;
using Engine.Interfaces._3D;

namespace Zeldo.Entities.Windmill
{
	public class MotorTree
	{
		private MotorNode root;
		private float rotation;

		public MotorTree(MotorNode root)
		{
			Debug.Assert(root != null, "Motor tree root can't be null.");

			this.root = root;
		}

		public float AngularVelocity { get; set; }

		public void PreStep(float step)
		{
			rotation += AngularVelocity * step;

			// This always keeps the net rotation between zero and two pi.
			if (rotation > Constants.TwoPi)
			{
				rotation -= Constants.TwoPi;
			}
			else if (rotation < 0)
			{
				rotation += Constants.TwoPi;
			}

			root.Apply(rotation);
		}
	}
}
