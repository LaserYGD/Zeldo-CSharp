using System;
using System.Linq;
using Engine.Graphics._3D;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Animation
{
	public class Skeleton : IMeshUser
	{
		private vec3[] defaultPose;

		public Skeleton(string filename) : this(ContentCache.GetMesh(filename))
		{
		}
		
		public Skeleton(Mesh mesh, vec3[] defaultPose = null)
		{
			Mesh = mesh;
			Bones = new Bone[mesh.BoneIndexes.Max(p => Math.Max(p.x, p.y)) + 1];
			Orientation = quat.Identity;
			IsShadowCaster = true;

			for (int i = 0; i < Bones.Length; i++)
			{
				Bones[i] = new Bone();
			}

			DefaultPose = defaultPose;
		}

		public Mesh Mesh { get; }
		public Bone[] Bones { get; }

		// Note that setting a default pose updates all bones to match.
		// TODO: Consider using a full Transform for the default pose (rather than only positions).
		public vec3[] DefaultPose
		{
			get => defaultPose;
			set
			{
				defaultPose = value;

				if (value == null)
				{
					PoseOrigin = vec3.Zero;

					return;
				}

				PoseOrigin = defaultPose.Aggregate(vec3.Zero, (current, p) => current + p) / value.Length;

				// It's assumed that if a default pose is set programatically, it's size will match the number of bones
				// on the skeleton.
				for (int i = 0; i < value.Length; i++)
				{
					Bones[i].Position = value[i];
				}
			}
		}

		public vec3 PoseOrigin { get; set; }
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
