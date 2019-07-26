using System.Collections.Generic;
using Engine.Animation;
using Engine.Lighting;
using GlmSharp;

namespace Engine.Graphics._3D.Rendering
{
	public class SkeletonRenderer : AbstractRenderer3D<Skeleton>
	{
		public SkeletonRenderer(GlobalLight light) : base(light)
		{
		}

		public override List<Skeleton> RetrieveNext()
		{
			return null;
		}

		public override void Add(Skeleton item)
		{
		}

		public override void Remove(Skeleton item)
		{
		}

		public override void PrepareShadow()
		{
		}

		public override void Prepare()
		{
		}

		public override void Draw(Skeleton item, mat4? vp)
		{
		}
	}
}
