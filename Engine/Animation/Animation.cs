using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Animation
{
	public class Animation
	{
		public Animation(Timeline[] timelines, float length, bool isRepeating)
		{
			Timelines = timelines;
			Length = length;
			IsRepeating = isRepeating;
		}

		public Timeline[] Timelines { get; }

		public float Length { get; }

		public bool IsRepeating { get; }
	}
}
