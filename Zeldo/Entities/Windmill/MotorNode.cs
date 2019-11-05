using System.Collections.Generic;
using GlmSharp;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Windmill
{
	public class MotorNode
	{
		private Entity owner;
		private quat baseOrientation;
		private float radius;

		public MotorNode(Entity owner, float radius)
		{
			this.owner = owner;
			this.radius = radius;

			// This assumes that the target's default (unrotated) orientation is set correctly when the node is
			// constructed.
			baseOrientation = owner.Orientation;
		}

		public List<MotorNode> Children { get; } = new List<MotorNode>();

		public void Apply(float rotation, float step)
		{
			// TODO: This causes quaternions to be computed many times. Could possibly be optimized by mapping rotations to quats? Might be fine.
			// This assumes that all models within the tree are oriented along the flat XZ plane by default.
			owner.Step(baseOrientation * quat.FromAxisAngle(rotation, vec3.UnitY), step);

			foreach (MotorNode child in Children)
			{
				// TODO: Are radii being handled properly, or do distances need to be measured?
				// This calculation maintains arc speed where the two objects touch (and flips the sign).
				child.Apply(-rotation * radius / child.radius, step);
			}
		}
	}
}
