using Engine.Core._3D;
using Engine.Lighting;
using Engine.Shaders;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics._3D.Rendering
{
	public class ModelRenderer : MapRenderer3D<Mesh, Model>
	{
		private uint bufferId;
		private uint indexId;

		// These sizes are updated as data is buffered to the GPU. The data isn't actually stored here.
		private int bufferSize;
		private int indexSize;
		private int maxIndex;

		public ModelRenderer(GlobalLight light) : base(light)
		{
			int bufferCapacity = Properties.GetInt("model.buffer.capacity");
			int indexCapacity = Properties.GetInt("model.index.capacity");

			GLUtilities.AllocateBuffers(bufferCapacity, indexCapacity, out bufferId, out indexId, GL_STATIC_DRAW);

			var shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "ModelShadow.vert");
			shader.Attach(ShaderTypes.Fragment, "ModelShadow.frag");
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.Initialize();
			shader.Use();
			shader.SetUniform("textureSampler", 0);
			shader.SetUniform("shadowSampler", 1);

			Bind(shader, bufferId, indexId);
		}
		
		public override unsafe void Add(Model item)
		{
			Add(item.Mesh, item);

			Mesh mesh = item.Mesh;

			var points = mesh.Points;
			var source = mesh.Source;
			var normals = mesh.Normals;
			var vertices = mesh.Vertices;

			float[] buffer = new float[vertices.Length * 8];

			for (int i = 0; i < vertices.Length; i++)
			{
				var v = vertices[i];
				var p = points[v.x];
				var s = source[v.y];
				var n = normals[v.z];

				int start = i * 8;

				buffer[start] = p.x;
				buffer[start + 1] = p.y;
				buffer[start + 2] = p.z;
				buffer[start + 3] = s.x;
				buffer[start + 4] = s.y;
				buffer[start + 5] = n.x;
				buffer[start + 6] = n.y;
				buffer[start + 7] = n.z;
			}

			ushort[] indices = mesh.Indices;

			int size = sizeof(float) * buffer.Length;
			int localIndexSize = sizeof(ushort) * indices.Length;

			// Each mesh only needs to be buffered to GPU memory once (the first time it's used).
			if (mesh.Handle != null)
			{
				return;
			}

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

		public override void Remove(Model item)
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
			glEnable(GL_CULL_FACE);
			glCullFace(GL_BACK);

			base.Prepare();
		}

		protected override void Apply(Mesh key)
		{
			key.Texture.Bind(0);
		}

		public override unsafe void Draw(Model item, mat4? vp)
		{
			PrepareShader(item, vp);

			var handle = item.Mesh.Handle;

			glDrawElementsBaseVertex(GL_TRIANGLES, (uint)handle.Count, GL_UNSIGNED_SHORT, (void*)handle.Offset,
				handle.BaseVertex);
		}
	}
}
