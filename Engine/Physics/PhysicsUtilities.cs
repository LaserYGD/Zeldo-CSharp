using Engine.Utility;
using GlmSharp;
using Jitter;
using Jitter.Dynamics;
using Jitter.LinearMath;

namespace Engine.Physics
{
	public static class PhysicsUtilities
	{
		public static RaycastResults Raycast(World world, vec3 start, vec3 direction, float range)
		{
			return Raycast(world, null, start, direction, range);
		}

		public static RaycastResults Raycast(World world, RigidBody body, vec3 start, vec3 direction, float range)
		{
			JVector jStart = start.ToJVector();
			JVector jDirection = direction.ToJVector() * range;
			JVector normal;
			JVector[] triangle;

			var system = world.CollisionSystem;

			float fraction;

			bool success = body != null
				? system.Raycast(body, jStart, jDirection, out normal, out fraction, out triangle)
				: system.Raycast(jStart, jDirection, (b, n, f) => true, out body, out normal,
					out fraction, out triangle);

			if (!success)
			{
				return null;
			}

			vec3[] tVectors = null;

			// Triangle will only be set if a triangle mesh was hit.
			if (triangle != null)
			{
				tVectors = new vec3[3];

				for (int i = 0; i < 3; i++)
				{
					tVectors[i] = triangle[i].ToVec3();
				}
			}

			return new RaycastResults(body, start + direction * fraction, Utilities.Normalize(normal.ToVec3()),
				tVectors);
		}
	}
}
