using GlmSharp;
using Jitter;
using Jitter.Dynamics;
using Jitter.LinearMath;

namespace Engine.Physics
{
	public static class PhysicsUtilities
	{
		public static RaycastResults Raycast(World world, vec3 start, vec3 direction)
		{
			world.CollisionSystem.Raycast(start.ToJVector(), direction.ToJVector(), (body, normal, fraction) => true,
				out RigidBody resultBody, out JVector resultNormal, out float resultFraction);

			return null;
		}
	}
}
