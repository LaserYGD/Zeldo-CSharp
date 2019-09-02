using System;
using Engine.Animation;
using Engine.Lighting;
using Engine.Shaders;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics._3D.Rendering
{
	public class SkeletonRenderer : MeshRenderer<Skeleton>
	{
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
			shader.AddAttribute<int>(1, GL_INT, ShaderAttributeFlags.IsInteger);

			Bind(bufferId, indexId);
		}

		protected override float[] GetData(Mesh mesh)
		{
			var points = mesh.Points;
			var source = mesh.Source;
			var normals = mesh.Normals;
			var vertices = mesh.Vertices;
			var boneIndexes = mesh.BoneIndexes;
			var boneWeights = mesh.BoneWeights;

			// The skeletal shader uses ten floats, two shorts, and an integer, which take up the same combined space
			// as twelve floats.
			float[] buffer = new float[vertices.Length * 12];

			for (int i = 0; i < vertices.Length; i++)
			{
				var v = vertices[i];
				var p = points[v.x];
				var s = source[v.y];
				var n = normals[v.z];
				var w = boneWeights[v.z];
				var d = boneIndexes[v.z];

				int start = i * 12;

				byte[] b1 = BitConverter.GetBytes((short)d.x);
				byte[] b2 = BitConverter.GetBytes((short)d.y);
				byte[] b3 = BitConverter.GetBytes(d.y == -1 ? 1 : 2);

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
				buffer[start + 11] = BitConverter.ToSingle(b3, 0);
			}

			return buffer;
		}

		public override void Draw(Skeleton item, mat4? vp)
		{
		}
	}
}
