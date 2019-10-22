using Engine;

namespace Zeldo.Physics
{
	public static class PhysicsConstants
	{
		static PhysicsConstants()
		{
			EdgeForgiveness = Properties.GetFloat("edge.forgiveness");
			WallThreshold = Properties.GetFloat("wall.threshold");
		}

		public static float EdgeForgiveness { get; }
		public static float WallThreshold { get; }
	}
}
