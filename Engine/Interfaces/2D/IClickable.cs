using GlmSharp;

namespace Engine.Interfaces._2D
{
	public interface IClickable : IBoundable2D
	{
		void OnHover(ivec2 mouseLocation);
		void OnUnhover();
		void OnClick(ivec2 mouseLocation);

		bool Contains(ivec2 mouseLocation);
	}
}
