using Engine.Core._3D;
using Engine.Shaders;
using static Engine.GL;

namespace Engine.Graphics._3D.Renderers
{
	public class ModelRenderer : AbstractRenderer3D<Mesh, Model>
	{
		private Shader modelShader;
		private Texture defaultTexture;

		private uint bufferId;
		private uint indexId;

		// These sizes are updated as data is buffered to the GPU. The data isn't actually stored here.
		private int bufferSize;
		private int indexSize;
		private int maxIndex;

		public ModelRenderer()
		{
			GLUtilities.AllocateBuffers(bufferSize, indexSize, out bufferId, out indexId, GL_STATIC_DRAW);

			modelShader = new Shader();
			modelShader.Attach(ShaderTypes.Vertex, "ModelShadow.vert");
			modelShader.Attach(ShaderTypes.Fragment, "ModelShadow.frag");
			modelShader.AddAttribute<float>(3, GL_FLOAT);
			modelShader.AddAttribute<float>(2, GL_FLOAT);
			modelShader.AddAttribute<float>(3, GL_FLOAT);
			modelShader.CreateProgram();
			modelShader.Bind(bufferId, indexId);
			modelShader.Use();
			modelShader.SetUniform("shadowSampler", 0);
			modelShader.SetUniform("textureSampler", 1);

			defaultTexture = ContentCache.GetTexture("Grey.png");
		}

		public override void Dispose()
		{
			modelShader.Dispose();

			GLUtilities.DeleteBuffers(bufferId, indexId);
		}

		protected override void Apply(Mesh key)
		{
		}

		public override void Add(Model item)
		{
			Add(item.Mesh, item);
		}

		public override void Remove(Model item)
		{
			Remove(item.Mesh, item);
		}

		public override void PrepareShadow()
		{
			glEnable(GL_CULL_FACE);
			glCullFace(GL_FRONT);
		}

		public override void Prepare()
		{
			glEnable(GL_CULL_FACE);
			glCullFace(GL_BACK);
		}

		public override unsafe void Draw(Mesh key)
		{
			MeshHandle handle = null;

			glDrawElementsBaseVertex(GL_TRIANGLES, (uint)handle.Count, GL_UNSIGNED_SHORT, (void*)handle.Offset,
				handle.BaseVertex);
		}
	}
}
