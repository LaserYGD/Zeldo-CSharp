
namespace Engine.Interfaces
{
	public interface IComponent : IDynamic
	{
		bool IsComplete { get; }
	}
}
