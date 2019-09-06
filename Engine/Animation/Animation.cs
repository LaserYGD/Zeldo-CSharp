using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Animation
{
	public class Animation
	{
		public Animation(Timeline[] timelines, float length, bool isLooped)
		{
			Timelines = timelines;
			Length = length;
			IsLooped = isLooped;
		}

		public Timeline[] Timelines { get; }

		public float Length { get; }

		public bool IsLooped { get; }
	}
}
