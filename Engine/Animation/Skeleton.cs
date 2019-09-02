using System;
using System.Linq;
using Engine.Graphics._3D;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Animation
{
	public class Skeleton : IRenderable3D
	{
		public Skeleton(string filename) : this(ContentCache.GetMesh(filename))
		{
		}

		public Skeleton(Mesh mesh)
		{
			Mesh = mesh;
			Bones = new Bone[mesh.BoneIndexes.Max(p => Math.Max(p.x, p.y))];
		}

		public Mesh Mesh { get; }
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
