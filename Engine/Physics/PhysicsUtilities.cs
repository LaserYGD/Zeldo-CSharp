using Engine.Utility;
using GlmSharp;
using Jitter;
using Jitter.Dynamics;
using Jitter.LinearMath;

namespace Engine.Physics
{
	public static class PhysicsUtilities
	{
		public static RaycastResults Raycast(World world, vec3 start, vec3 end)
		{
			return RaycastInternal(world, null, start, end - start);
		}

		public static RaycastResults Raycast(World world, vec3 start, vec3 direction, float range)
		{
			return RaycastInternal(world, null, start, direction * range);
		}

		public static RaycastResults Raycast(World world, RigidBody body, vec3 start, vec3 end)
		{
			return RaycastInternal(world, body, start, end - start);
		}

		public static RaycastResults Raycast(World world, RigidBody body, vec3 start, vec3 direction, float range)
		{
			return RaycastInternal(world, body, start, direction * range);
		}

		private static RaycastResults RaycastInternal(World world, RigidBody body, vec3 start, vec3 ray)
		{
			JVector jStart = start.ToJVector();
			
			// Note that Jitter's Raycast signature below calls one of its parameters "rayDirection", but that vector
			// isn't meant to be normalized (meaning that it's actually just a ray from start to end).
			JVector jDirection = ray.ToJVector();
			JVector normal;
			JVector[] triangle;

			var system = world.CollisionSystem;

			float fraction;

			bool success = body != null
				? system.Raycast(body, jStart, jDirection, out normal, out fraction, out triangle)
				: system.Raycast(jStart, jDirection, (b, n, f) => true, out body, out normal,
					out fraction, out triangle);

			if (!success || fraction > 1)
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

			return new RaycastResults(body, start + jDirection.ToVec3() * fraction,
				Utilities.Normalize(normal.ToVec3()), tVectors);
		}
	}
}
