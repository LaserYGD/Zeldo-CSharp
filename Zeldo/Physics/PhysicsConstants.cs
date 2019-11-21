using Engine;
using Engine.Props;

namespace Zeldo.Physics
{
	public static class PhysicsConstants
	{
		static PhysicsConstants()
		{
			var accessor = Properties.Access();

			// TODO: These should be reloadable (I think).
			EdgeForgiveness = accessor.GetFloat("edge.forgiveness");
			Gravity = accessor.GetFloat("gravity");
			StepThreshold = accessor.GetFloat("step.threshold");
			WallThreshold = accessor.GetFloat("wall.threshold");
		}

		public static float EdgeForgiveness { get; }
		public static float Gravity { get; }
		public static float StepThreshold { get; }
		public static float WallThreshold { get; }
	}
}
