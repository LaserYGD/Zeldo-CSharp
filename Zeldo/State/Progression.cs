using System.Diagnostics;
using System.Linq;

namespace Zeldo.State
{
	public class Progression
	{


		public ProgressionEvent[] Events { get; }
		public ProgressionPool[] Pools { get; }

		public float ComputePercentage()
		{
			float eventSum = Events.Where(e => e.Flag).Sum(e => e.Contribution);
			float poolSum = Pools.Sum(p => (float)p.Current / p.Max * p.Contribution);
			float sum = eventSum + poolSum;

			Debug.Assert(sum >= 0 && sum <= 100, "Computed percentage must be between 0 and 100.");

			return sum;
		}
	}
}
