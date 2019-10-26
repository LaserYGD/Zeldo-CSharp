using Zeldo.Entities.Core;

namespace Zeldo.Control
{
	public abstract class AbstractController
	{
		protected AbstractController(Actor parent)
		{
			Parent = parent;
		}

		protected Actor Parent { get; }

		public virtual void PreStep(float step)
		{
		}

		public virtual void MidStep(float step)
		{
		}

		public virtual void PostStep(float step)
		{
		}
	}
}
