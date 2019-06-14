using Engine.Interfaces;
using Zeldo.Entities.Core;

namespace Zeldo.Controllers
{
	public abstract class CharacterController : IDynamic
	{
		protected Actor Parent { get; private set; }

		// Attaching the parent through a function (rather than the constructor) allows the controller to properly set
		// parent velocity (such as transitioning from a normal room to a spiral staircase, or vice versa).
		public virtual void Attach(Actor parent)
		{
			Parent = parent;
		}

		public abstract void Update(float dt);
	}
}
