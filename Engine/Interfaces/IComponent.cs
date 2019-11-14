
namespace Engine.Interfaces
{
	// TODO: Consider making components disposable.
	public interface IComponent : IDynamic
	{
		bool IsComplete { get; }
	}
}
