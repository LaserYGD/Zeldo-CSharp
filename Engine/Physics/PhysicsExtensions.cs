using System;
using GlmSharp;
using Jitter.LinearMath;

namespace Engine.Physics
{
	public static class PhysicsExtensions
	{
		public static vec3 ToVec3(this JVector v)
		{
			return new vec3(v.X, v.Y, v.Z);
		}

		public static JVector ToJVector(this vec3 v)
		{
			return new JVector(v.x, v.y, v.z);
		}

		public static quat ToQuat(this JMatrix m)
		{
			var q = JQuaternion.CreateFromMatrix(m);

			return new quat(q.X, q.Y, q.Z, q.W);
		}

		public static JMatrix ToJMatrix(this quat q)
		{
			mat3 m = new mat3(q);

			return new JMatrix(m.m00, m.m01, m.m02, m.m10, m.m11, m.m12, m.m20, m.m21, m.m22);
		}

		// This is primarily used for tracking relative rotation on moving platforms.
		public static float ComputeYaw(this JMatrix matrix)
		{
			// See https://stackoverflow.com/a/4341489/7281613.
			JVector t = JVector.Transform(JVector.Left, matrix);
			JVector f = t - JVector.Dot(t, JVector.Up) * JVector.Up;
			f.Normalize();

			float angle = (float)Math.Acos(JVector.Dot(JVector.Left, f));
			float d = JVector.Dot(JVector.Up, JVector.Cross(JVector.Left, f));

			if (d < 0)
			{
				angle = Constants.TwoPi - angle;
			}

			return angle;
		}
	}
}
