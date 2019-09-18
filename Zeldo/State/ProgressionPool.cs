using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeldo.State
{
	public class ProgressionPool
	{
		public ProgressionPool(int max, float contribution)
		{
			Max = max;
			Contribution = contribution;
		}

		public int Current { get; set; }
		public int Max { get; }

		public float Contribution { get; }
	}
}
