using System.Collections.Generic;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Zeldo.Entities.Windmill
{
	public class MotorNode
	{
		private IOrientable target;
		private quat baseOrientation;
		private float radius;

		public MotorNode(IOrientable target, float radius)
		{
			this.target = target;
			this.radius = radius;

			// This assumes that the target's default (unrotated) orientation is set correctly when the node is
			// constructed.
			baseOrientation = target.Orientation;
		}

		public List<MotorNode> Children { get; } = new List<MotorNode>();

		public void Apply(float rotation)
		{
			// This assumes that all models within the tree are oriented along the flat XZ plane by default.
			target.Orientation = baseOrientation * quat.FromAxisAngle(rotation, vec3.UnitY);

			foreach (MotorNode child in Children)
			{
				// TODO: Are radii being handled properly, or do distances need to be measured?
				// This calculation maintains arc speed where the two objects touch (and flips the sign).
				child.Apply(-rotation * radius / child.radius);
			}
		}
	}
}
