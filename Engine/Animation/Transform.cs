using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;

namespace Engine.Animation
{
	public class Transform
	{
		public static Transform Lerp(Transform t1, Transform t2, float t)
		{
			vec3 position = vec3.Lerp(t1.Position, t2.Position, t);
			quat orientation = quat.SLerp(t1.Orientation, t2.Orientation, t);

			return new Transform(position, orientation);
		}

		public Transform(vec3 position, quat orientation)
		{
			Position = position;
			Orientation = orientation;
		}

		public vec3 Position { get; }
		public quat Orientation { get; }
	}
}
