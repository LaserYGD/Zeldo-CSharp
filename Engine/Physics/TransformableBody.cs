using Engine.Interfaces._3D;
using GlmSharp;
using Jitter.Dynamics;

namespace Engine.Physics
{
	public class TransformableBody : ITransformable3D
	{
		public TransformableBody(RigidBody body)
		{
			Body = body;
		}

		public RigidBody Body { get; }

		public vec3 Position
		{
			get => Body.Position.ToVec3();
			set => Body.Position = value.ToJVector();
		}

		public quat Orientation
		{
			get => Body.Orientation.ToQuat();
			set => Body.Orientation = value.ToJMatrix();
		}

		public void SetTransform(vec3 position, quat orientation)
		{
			Position = position;
			Orientation = orientation;
		}
	}
}
