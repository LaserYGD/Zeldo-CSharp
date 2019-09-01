using System;
using Engine;
using Engine.Shaders;
using Engine.Structures;
using GlmSharp;
using Zeldo.Entities.Core;
using static Engine.GL;

namespace Zeldo
{
	public class TentacleTester
	{
		private const int Bones = 20;

		private Curve3D curve;
		private Shader shader;

		public TentacleTester(Scene scene)
		{
			curve = new Curve3D();

			shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "Skeletal.vert");
			shader.Attach(ShaderTypes.Fragment, "ModelShadow.frag");
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<short>(2, GL_SHORT, ShaderAttributeFlags.IsInteger);
			shader.AddAttribute<int>(1, GL_INT, ShaderAttributeFlags.IsInteger);
			shader.Initialize();
			shader.Use();
			shader.SetUniform("shadowSampler", 0);
			shader.SetUniform("textureSampler", 1);

			var mesh = ContentCache.GetMesh("Tentacle.obj");
			var points = mesh.Points;
			var vertices = mesh.Vertices;

			ivec2[] boneIndexes = new ivec2[vertices.Length];
			vec2[] boneWeights = new vec2[vertices.Length];

			const float Range = 10;
			const float SegmentLength = Range * 2 / (Bones - 1);

			for (int i = 0; i < vertices.Length; i++)
			{
				float x = points[vertices[i].x].x;
				float weight = ((x + Range) % SegmentLength) / SegmentLength;

				if (weight > 0.5f)
				{
					weight = 1 - weight;
				}

				// Each segment spans two bones.
				int index = (int)Math.Floor((x + Range) / SegmentLength);

				boneIndexes[i] = new ivec2(index, index + 1);
				boneWeights[i] = new vec2(weight, 1 - weight);
			}
		}

		public void Draw()
		{
			var points = curve.Evaluate(Bones - 1);
			var bones = new mat4[points.Length];

			for (int i = 0; i < points.Length; i++)
			{
				bones[i] = mat4.Translate(points[i]);
			}

			shader.Use();
			shader.SetUniform("bones", bones);
		}
	}
}
