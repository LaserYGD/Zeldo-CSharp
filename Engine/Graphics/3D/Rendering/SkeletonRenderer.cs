using System;
using System.Linq;
using Engine.Animation;
using Engine.Lighting;
using Engine.Shaders;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics._3D.Rendering
{
	public class SkeletonRenderer : MeshRenderer<Skeleton>
	{
		private Shader shadowShader;

		public SkeletonRenderer(GlobalLight light) : base(light, "skeletal")
		{
			shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "Skeletal.vert");
			shader.Attach(ShaderTypes.Fragment, "ModelShadow.frag");
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<short>(2, GL_SHORT, ShaderAttributeFlags.IsInteger);

			shadowShader = new Shader();
			shadowShader.Attach(ShaderTypes.Vertex, "ShadowMapSkeletal.vert");
			shadowShader.Attach(ShaderTypes.Fragment, "ShadowMap.frag");
			shadowShader.Initialize();
			shadowShader.Use();
			shadowShader.SetUniform("image", 0);

			Bind(bufferId, indexId);
		}

		public override Shader ShadowShader => shadowShader;

		public override void Dispose()
		{
			shadowShader.Dispose();

			base.Dispose();
		}

		protected override unsafe int InitializeShadowVao(uint stride)
		{
			glVertexAttribPointer(0, 3, GL_FLOAT, false, stride, (void*)0);
			glVertexAttribPointer(1, 2, GL_FLOAT, false, stride, (void*)(sizeof(float) * 3));
			glVertexAttribPointer(2, 2, GL_FLOAT, false, stride, (void*)(sizeof(float) * 8));
			glVertexAttribIPointer(3, 2, GL_SHORT, stride, (void*)(sizeof(float) * 10));

			return 4;
		}

		protected override float[] GetData(Mesh mesh)
		{
			var points = mesh.Points;
			var source = mesh.Source;
			var normals = mesh.Normals;
			var vertices = mesh.Vertices;
			var boneIndexes = mesh.BoneIndexes;
			var boneWeights = mesh.BoneWeights;

			// The skeletal shader uses ten floats and two shorts, which take up the same combined space as 11 floats.
			float[] buffer = new float[vertices.Length * 11];

			for (int i = 0; i < vertices.Length; i++)
			{
				var v = vertices[i];
				var p = points[v.x];
				var s = source[v.y];
				var n = normals[v.z];

				// Positions, normals, and source coordinates are stored in distinct lists, then indexed by vertex. In
				// contrast, bone data is stored directly (i.e. one entry per vertex).
				var w = boneWeights[i];
				var d = boneIndexes[i];
				
				int start = i * 11;

				byte[] b1 = BitConverter.GetBytes((short)d.x);
				byte[] b2 = BitConverter.GetBytes((short)d.y);

				buffer[start] = p.x;
				buffer[start + 1] = p.y;
				buffer[start + 2] = p.z;
				buffer[start + 3] = s.x;
				buffer[start + 4] = s.y;
				buffer[start + 5] = n.x;
				buffer[start + 6] = n.y;
				buffer[start + 7] = n.z;
				buffer[start + 8] = w.x;
				buffer[start + 9] = w.y;

				// Both indexes (interpreted as shorts by OpenGL) are combined into the same float.
				buffer[start + 10] = BitConverter.ToSingle(new[] { b1[0], b1[1], b2[0], b2[1] }, 0);
			}

			return buffer;
		}

		public override void Draw(Skeleton item, mat4? vp)
		{
			var bones = item.Bones;

			vec3[] bonePositions = bones.Select(b => b.Position).ToArray();
			vec4[] boneOrientations = item.Bones.Select(b => b.Orientation.ToVec4()).ToArray();

			var activeShader = vp.HasValue ? shader : shadowShader;

			activeShader.SetUniform("poseOrigin", item.PoseOrigin);
			activeShader.SetUniform("defaultPose", item.DefaultPose);
			activeShader.SetUniform("bonePositions", bonePositions);
			activeShader.SetUniform("boneOrientations", boneOrientations);

			base.Draw(item, vp);
		}
	}
}
