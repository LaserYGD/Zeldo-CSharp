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
		private const float HoverHeight = 3.5f;

		private Scene scene;
		private Curve3D curve;
		private Skeleton skeleton;
		private SpringPoint3D[] springs;

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

			// Initialize springs.
			vec3 target = scene.GetEntities(EntityGroups.Player)[0].Position;
			vec3[] springPoints =
			{
				new vec3(halfRange * 2, HoverHeight, 0),
				new vec3(halfRange, HoverHeight, 0),
				new vec3(target.x, HoverHeight, target.z)
			};

			springs = springPoints.Select(p => new SpringPoint3D(p, 0.85f, 0.85f)).ToArray();
			skeleton = new Skeleton(mesh, defaultPose);
			scene.Renderer.Add(skeleton);
		}

		public void Update(float dt)
		{
			vec3 target = scene.GetEntities(EntityGroups.Player)[0].Position;
			target.y = HoverHeight;

			springs[2].Target = target;

			foreach (var spring in springs)
			{
				spring.Update(dt);
			}

			var list = curve.ControlPoints;
			list.Clear();
			list.AddRange(springs.Select(s => s.Position));

			var points = curve.Evaluate(Bones - 1);
			var bones = skeleton.Bones;

			for (int i = 0; i < points.Length; i++)
			{
				bones[i].Position = points[i];
			}
		}
	}
}
