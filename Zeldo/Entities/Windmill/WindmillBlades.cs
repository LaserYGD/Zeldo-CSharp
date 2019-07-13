using Zeldo.Entities.Core;

namespace Zeldo.Entities.Windmill
{
	public class WindmillBlades : Entity
	{
		private MotorTree motorTree;

		public WindmillBlades() : base(EntityGroups.Object)
		{
			IsPersistent = true;
		}

		public override void Initialize(Scene scene)
		{
			Components.Add(motorTree);
		}
	}
}
