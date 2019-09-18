using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeldo.State
{
	public class ProgressionEvent
	{
		public ProgressionEvent(float contribution)
		{
			Contribution = contribution;
		}

		public bool Flag { get; set; }

		public float Contribution { get; }
	}
}
