using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Animation
{
	public class Timeline
	{
		public Timeline(Keyframe[] keyframes)
		{
			Keyframes = keyframes;
		}

		public Keyframe[] Keyframes { get; }
	}
}
