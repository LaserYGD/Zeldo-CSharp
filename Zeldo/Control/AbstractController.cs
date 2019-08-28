using Engine.Interfaces;
using Zeldo.Entities.Core;

namespace Zeldo.Control
{
	public abstract class AbstractController : IDynamic
	{
		protected AbstractController(Actor parent)
		{
			Parent = parent;
		}

		protected Actor Parent { get;}

		public abstract void Update(float dt);
	}
}
