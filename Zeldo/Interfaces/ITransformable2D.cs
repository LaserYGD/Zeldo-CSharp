using Engine.Interfaces._2D;
using GlmSharp;

namespace Zeldo.Interfaces
{
	public interface ITransformable2D : IPositionable2D, IRotatable
	{
		float Elevation { get; set; }

		void SetTransform(vec2 position, float elevation, float rotation);
	}
}
