using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Animation
{
	public class Skeleton : IRenderable3D
	{
		public Skeleton(Bone[] bones)
		{
			Bones = bones;
		}

		public Bone[] Bones { get; }

		public vec3 Position { get; set; }
		public quat Orientation { get; set; }
		public mat4 WorldMatrix { get; private set; }

		public bool IsShadowCaster { get; set; }

		public void SetTransform(vec3 position, quat orientation)
		{
			Position = position;
			Orientation = orientation;
		}

		public void RecomputeWorldMatrix()
		{
			WorldMatrix = mat4.Translate(Position) * Orientation.ToMat4;
		}
	}
}
