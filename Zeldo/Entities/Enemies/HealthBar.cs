using Zeldo.Entities.Core;

namespace Zeldo.Entities.Enemies
{
	// TODO: Consider creating an alternate entity class targeted at UI-based objects.
	// TODO: Consider the best way to display health bars in a 3D space (or whether they should be included at all).
	public class HealthBar : Entity
	{
		public HealthBar() : base(EntityGroups.Interface)
		{
		}
	}
}
