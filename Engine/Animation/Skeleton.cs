using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Animation
{
	// Note that skeletons are intentionally not transformable themselves. The idea is that skeletons manage the local
	// transforms of each bone (usually based off an animation player), while global transform is still controlled by
	// the 3D model.
	public class Skeleton
	{
		private Bone[] bones;

		public Skeleton(Bone[] bones)
		{
			this.bones = bones;
		}

		public Bone[] Bones { get; }
	}
}
