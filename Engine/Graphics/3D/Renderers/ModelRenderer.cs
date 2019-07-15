using System.Collections.Generic;
using Engine.Core._3D;

namespace Engine.Graphics._3D.Renderers
{
	public class ModelRenderer : AbstractRenderer3D<Mesh, Model>
	{
		private Dictionary<Mesh, Model> map;

		public ModelRenderer(MasterRenderer3D parent) : base(parent)
		{
		}

		public override void Dispose()
		{
		}

		public override List<Model> RetrieveNext()
		{
			return null;
		}

		public override void PrepareShadow()
		{
		}

		public override void Prepare()
		{
		}

		public override void Draw(Mesh key)
		{
		}
	}
}
