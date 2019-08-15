using Engine.Interfaces;

namespace Engine.Core
{
	public abstract class Component : IDynamic
	{
		public bool IsComplete { get; set; }

		public abstract void Update(float dt);
	}
}
