using Engine;

namespace Zeldo.Physics
{
	public static class PhysicsConstants
	{
		static PhysicsConstants()
		{
			EdgeForgiveness = Properties.GetFloat("edge.forgiveness");
			Gravity = Properties.GetFloat("gravity");
			StepThreshold = Properties.GetFloat("step.threshold");
			WallThreshold = Properties.GetFloat("wall.threshold");
		}

		public static float EdgeForgiveness { get; }
		public static float Gravity { get; }
		public static float StepThreshold { get; }
		public static float WallThreshold { get; }
	}
}
