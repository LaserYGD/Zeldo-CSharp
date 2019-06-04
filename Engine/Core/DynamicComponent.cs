using Engine.Interfaces;

namespace Engine.Core
{
	public abstract class DynamicComponent : IDynamic
	{
		public virtual bool Complete { get; protected set; }

		public abstract void Update(float dt);
	}
}
