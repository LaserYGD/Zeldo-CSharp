using Engine.Interfaces._3D;
using Engine.Lighting;
using static Engine.GL;

namespace Engine.Graphics._3D.Rendering
{
	public abstract class MeshRenderer<T> : AbstractRenderer3D<Mesh, T> where T : IMeshUser
	{
		private int bufferSize;
		private int indexSize;
		private int maxIndex;

		protected MeshRenderer(GlobalLight light, string property) : base(light)
		{
			int bufferCapacity = Properties.GetInt(property + ".buffer.capacity");
			int indexCapacity = Properties.GetInt(property + ".index.capacity");

			GLUtilities.AllocateBuffers(bufferCapacity, indexCapacity, out bufferId, out indexId, GL_STATIC_DRAW);
		}

		public override unsafe void Add(T item)
		{
			Mesh mesh = item.Mesh;

			Add(mesh, item);

			// Each mesh only needs to be buffered to GPU memory once (the first time it's used).
			if (mesh.Handle != null)
			{
				return;
			}

			float[] data = GetData(mesh);
			ushort[] indices = mesh.Indices;

			int size = sizeof(float) * data.Length;
			int localIndexSize = sizeof(ushort) * indices.Length;

			var handle = new MeshHandle(indices.Length, indexSize, maxIndex);
			mesh.Handle = handle;

			maxIndex += mesh.MaxIndex + 1;
			glBindBuffer(GL_ARRAY_BUFFER, bufferId);

			fixed (float* address = &data[0])
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

		protected abstract float[] GetData(Mesh mesh);

		public override void Remove(T item)
		{
			Remove(item.Mesh, item);
		}

		protected override void Apply(Mesh key)
		{
			key.Texture.Bind(0);
		}

		public override void PrepareShadow()
		{
			glEnable(GL_CULL_FACE);
			glCullFace(GL_FRONT);

			base.PrepareShadow();
		}

		public override void Prepare()
		{
			// Note that face culling will already be enabled here (via PrepareShadow()).
			glCullFace(GL_BACK);

			base.Prepare();
		}
	}
}
