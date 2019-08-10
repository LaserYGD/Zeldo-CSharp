using Engine.Interfaces;
using Zeldo.Entities.Core;

namespace Zeldo.Control
{
	public abstract class CharacterController : IDynamic
	{
		protected CharacterController(Actor parent = null)
		{
			Parent = parent;
		}

		public Actor Parent { get; set; }

		public abstract void Update(float dt);
	}
}
