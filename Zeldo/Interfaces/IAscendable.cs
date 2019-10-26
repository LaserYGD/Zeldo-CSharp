using GlmSharp;

namespace Zeldo.Interfaces
{
	public interface IAscendable
	{
		// This allows ascension targets to better control player movement (rather than enforcing strictly vertical
		// movement).
		vec3 ComputeAscension(float t);
	}
}
