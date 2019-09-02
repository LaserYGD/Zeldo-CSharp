using System;
using Engine.Animation;
using Engine.Lighting;
using Engine.Shaders;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics._3D.Rendering
{
	public class SkeletonRenderer : MapRenderer3D<Mesh, Skeleton>
	{
		private uint bufferId;
		private uint indexId;

		private int bufferSize;
		private int indexSize;
		private int maxIndex;

		public SkeletonRenderer(GlobalLight light) : base(light)
		{
			int bufferCapacity = Properties.GetInt("skeletal.buffer.capacity");
			int indexCapacity = Properties.GetInt("skeletal.index.capacity");

			GLUtilities.AllocateBuffers(bufferCapacity, indexCapacity, out bufferId, out indexId, GL_STATIC_DRAW);

			Shader shader = new Shader();
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

			Bind(Shader, bufferId, indexId);
		}

		public override unsafe void Add(Skeleton item)
		{
			Mesh mesh = item.Mesh;

			Add(mesh, item);

			// Each skeleton (mesh + bone data) only needs to be buffered to GPU memory once (the first time it's
			// used).
			if (mesh.Handle != null)
			{
				return;
			}

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
				buffer[start + 10] = BitConverter.ToSingle(new [] { b1[0], b1[1], b2[0], b2[1] }, 0);
				buffer[start + 11] = BitConverter.ToSingle(b3, 0);
			}

			ushort[] indices = mesh.Indices;

			int size = sizeof(float) * buffer.Length;
			int localIndexSize = sizeof(ushort) * indices.Length;

			var handle = new MeshHandle(indices.Length, indexSize, maxIndex);
			mesh.Handle = handle;

			maxIndex += mesh.MaxIndex + 1;
			glBindBuffer(GL_ARRAY_BUFFER, bufferId);

			fixed (float* address = &buffer[0])
			{
				glBufferSubData(GL_ARRAY_BUFFER, bufferSize, (uint)size, address);
			}

			bufferSize += size;
			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);

			fixed (ushort* address = &indices[0])
			{
				glBufferSubData(GL_ELEMENT_ARRAY_BUFFER, indexSize, (uint)localIndexSize, address);
			}

			indexSize += localIndexSize;
		}

		public override void Remove(Skeleton item)
		{
			Remove(item.Mesh, item);
		}

		public override void PrepareShadow()
		{
			glEnable(GL_CULL_FACE);
			glCullFace(GL_FRONT);

			base.PrepareShadow();
		}

		public override void Prepare()
		{
			// TODO: This call to glEnable can probably be removed (since it'll already be enabled via the earlier call to PrepareShadow()).
			glEnable(GL_CULL_FACE);
			glCullFace(GL_BACK);

			base.Prepare();
		}

		public override void Draw(Skeleton item, mat4? vp)
		{
		}
	}
}
