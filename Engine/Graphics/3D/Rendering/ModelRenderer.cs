using Engine.Core._3D;
using Engine.Lighting;
using Engine.Shaders;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics._3D.Rendering
{
	public class ModelRenderer : MeshRenderer<Model>
	{
		public ModelRenderer(GlobalLight light) : base(light, "model")
		{
			shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "ModelShadow.vert");
			shader.Attach(ShaderTypes.Fragment, "ModelShadow.frag");
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<float>(3, GL_FLOAT);

			Bind(bufferId, indexId);
		}

		protected override float[] GetData(Mesh mesh)
		{
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

			return buffer;
		}
	}
}
