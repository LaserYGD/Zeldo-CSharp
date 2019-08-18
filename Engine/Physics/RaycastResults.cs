using GlmSharp;
using Jitter.Dynamics;

namespace Engine.Physics
{
	public class RaycastResults
	{
		public RaycastResults(RigidBody body, vec3 position, vec3 normal, vec3[] triangle)
		{
			Body = body;
			Position = position;
			Normal = normal;
			Triangle = triangle;
		}

		public RigidBody Body { get; }
		public vec3 Position { get; }
		public vec3 Normal { get; }
		public vec3[] Triangle { get; }
	}
}
