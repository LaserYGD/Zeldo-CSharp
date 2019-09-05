using System;
using System.Linq;
using Engine;
using Engine.Animation;
using Engine.Interfaces;
using Engine.Structures;
using GlmSharp;
using Zeldo.Entities.Core;

namespace Zeldo
{
	public class TentacleTester : IDynamic
	{
		private const int Bones = 40;

		private Scene scene;
		private Curve3D curve;
		private Skeleton skeleton;

		public TentacleTester(Scene scene)
		{
			this.scene = scene;

			var mesh = ContentCache.GetMesh("Tentacle.obj");
			var points = mesh.Points;
			var vertices = mesh.Vertices;

			ivec2[] boneIndexes = new ivec2[vertices.Length];
			vec2[] boneWeights = new vec2[vertices.Length];

			float halfRange = (points.Max(p => p.x) - points.Min(p => p.x)) / 2;
			float segmentLength = halfRange * 2 / (Bones - 1);
			
			// Compute bone indexes and weights.
			for (int i = 0; i < vertices.Length; i++)
			{
				float x = points[vertices[i].x].x;
				float weight = 1 - ((x + halfRange) % segmentLength) / segmentLength;

				// Each segment spans two bones.
				int index = (int)Math.Floor((x + halfRange) / segmentLength);

				boneIndexes[i] = new ivec2(index, Math.Min(index + 1, Bones - 1));
				boneWeights[i] = new vec2(weight, 1 - weight);
			}

			mesh.BoneIndexes = boneIndexes;
			mesh.BoneWeights = boneWeights;

			curve = new Curve3D();

			// Compute the default pose.
			vec3[] defaultPose = new vec3[Bones];

			for (int i = 0; i < Bones; i++)
			{
				defaultPose[i] = new vec3(-halfRange + segmentLength * i, 0, 0);
			}

			skeleton = new Skeleton(mesh, defaultPose);
			scene.Renderer.Add(skeleton);
		}

		private float offset = 0;

		public void Update(float dt)
		{
			var pose = skeleton.DefaultPose;

			vec3[] array =
			{
				pose[0],
				scene.GetEntities(EntityGroups.Player)[0].Position,
				pose.Last()
			};

			var list = curve.ControlPoints;
			list.Clear();
			list.AddRange(array);

			var points = curve.Evaluate(Bones - 1);
			var bones = skeleton.Bones;

			offset += dt * 1.5f;

			if (offset >= Constants.TwoPi)
			{
				offset -= Constants.TwoPi;
			}

			for (int i = 0; i < points.Length; i++)
			{
				float angle = Constants.TwoPi / bones.Length * 3 * i + offset;
				float y = (float)Math.Sin(angle) + 3;

				//bones[i].Position = points[i];
				var bone = bones[i];
				var p = bone.Position;
				p.y = y;
				bone.Position = p;
			}

			//skeleton.Bones[1].Position += vec3.UnitY * dt / 5;
			//skeleton.Bones[3].Position = scene.GetEntities(EntityGroups.Player)[0].Position;
			//skeleton.Bones[0].Orientation *= quat.FromAxisAngle(dt / 2, vec3.UnitX);
			//skeleton.Bones[1].Orientation *= quat.FromAxisAngle(dt / 2, vec3.UnitX);
			//skeleton.Bones[1].Orientation *= quat.FromAxisAngle(dt, vec3.UnitX);
			//skeleton.Bones[2].Orientation *= quat.FromAxisAngle(dt, vec3.UnitX);

			//bones[0].Position = -vec3.UnitY;
			//bones[1].Position = vec3.UnitY;
			//bones[2].Position = -vec3.UnitY;

			//for (int i = 0; i < bones.Length; i++)
			//{
			//	bones[i].Position += vec3.UnitY * dt / 5 * (i % 2 == 0 ? 1 : -1);
			//}

			for (int i = 0; i < bones.Length; i += 2)
			{
				//bones[i].Orientation *= quat.FromAxisAngle(dt / 2, vec3.UnitX);
			}
		}
	}
}
