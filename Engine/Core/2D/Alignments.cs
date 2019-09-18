using System;

namespace Engine.Core._2D
{
	[Flags]
	public enum Alignments
	{
		Center = 0,
		Left = 1,
		Right = 2,
		Top = 4,
		Bottom = 8,
		None = -1
	}
}
