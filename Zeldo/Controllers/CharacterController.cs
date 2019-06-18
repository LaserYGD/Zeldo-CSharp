using Engine.Interfaces;
using Zeldo.Entities.Core;

namespace Zeldo.Controllers
{
	public abstract class CharacterController : IDynamic
	{
		protected CharacterController(Actor parent)
		{
			Parent = parent;
		}

		protected Actor Parent { get; }

		public abstract void Update(float dt);
	}
}
