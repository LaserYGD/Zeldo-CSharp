using Engine.Interfaces._3D;
using Zeldo.Entities.Grabbable;

namespace Zeldo.Interfaces
{
	public interface IGrabbable : IPositionable3D
	{
		GrabTypes GrabType { get; }
	}
}
