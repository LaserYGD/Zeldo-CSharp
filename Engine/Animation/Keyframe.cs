using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Animation
{
	public class Keyframe
	{
		public Keyframe(Transform transform, float time, float duration)
		{
			Transform = transform;
			Time = time;
			Duration = duration;
		}

		public Transform Transform { get; }

		public float Time { get; }
		public float Duration { get; }
	}
}
