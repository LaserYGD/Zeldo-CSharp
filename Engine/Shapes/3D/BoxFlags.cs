using System;

namespace Engine.Shapes._3D
{
	[Flags]
	public enum BoxFlags
	{
		None = 0,

		// Note that fixed-vertical intentionally overlaps orientable (in terms of bits). Fixed-vertical boxes are by
		// definition orientable.
		IsFixedVertical = 3,
		IsOrientable = 1
	}
}
