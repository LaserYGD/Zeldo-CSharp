using Engine.Utility;
using GlmSharp;
using Jitter;
using Jitter.Dynamics;
using Jitter.LinearMath;

namespace Engine.Physics
{
	public static class PhysicsUtilities
	{
		public static bool Raycast(World world, vec3 start, vec3 end, out RaycastResults results)
		{
			return RaycastInternal(world, null, start, end - start, out results);
		}

		public static bool Raycast(World world, vec3 start, vec3 direction, float range, out RaycastResults results)
		{
			return RaycastInternal(world, null, start, direction * range, out results);
		}

		public static bool Raycast(World world, RigidBody body, vec3 start, vec3 end, out RaycastResults results)
		{
			return RaycastInternal(world, body, start, end - start, out results);
		}

		public static bool Raycast(World world, RigidBody body, vec3 start, vec3 direction, float range,
			out RaycastResults results)
		{
			return RaycastInternal(world, body, start, direction * range, out results);
		}

		private static bool RaycastInternal(World world, RigidBody body, vec3 start, vec3 ray,
			out RaycastResults results)
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
				results = null;

				return false;
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

			results = new RaycastResults(body, start + jDirection.ToVec3() * fraction,
				Utilities.Normalize(normal.ToVec3()), tVectors);

			return true;
		}
	}
}
