using System.Collections.Generic;
using Engine;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Zeldo.Entities.Windmill
{
	public class MotorNode
	{
		private IOrientable target;

		private quat baseOrientation;
		private vec3 axis;

		public float Radius { get; private set; }

		public List<MotorNode> Children { get; } = new List<MotorNode>();

		public void Attach(IOrientable target, float radius, vec3 axis)
		{
			this.target = target;
			this.axis = axis;

			// This assumes that 1) the target's rotation axis won't change once placed, and 2) the target will already
			// be in its default (unrotated) orientation when this function is called.
			baseOrientation = target.Orientation;
			Radius = radius;
		}

		public void Apply(float rotation)
		{
			target.Orientation = baseOrientation * quat.FromAxisAngle(rotation, axis);

			foreach (MotorNode child in Children)
			{
				// This calculation maintains arc speed where the two objects touch (and flips the sign).
				child.Apply(-rotation * Radius / child.Radius);
			}
		}
	}
}
