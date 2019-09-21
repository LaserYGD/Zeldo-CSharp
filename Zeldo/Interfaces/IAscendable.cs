using GlmSharp;

namespace Zeldo.Interfaces
{
	public interface IAscendable
	{
		// All ascension targets are assumed to be primarily vertical (such as ladders or lightly-swinging ropes).
		vec2 AscesionAxis { get; }

		float AscensionTop { get; }
		float AscensionBottom { get; }
	}
}
