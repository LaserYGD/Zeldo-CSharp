using Engine.Core._2D;
using Engine.Graphics;
using Engine.Graphics._2D;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Shaders;
using GlmSharp;
using static Engine.GL;

namespace Zeldo
{
	public class RaindropTester : IDynamic, IRenderable2D
	{
		private const int Width = 400;
		private const int Height = 800;

		private vec3[,] grid;
		private RenderTarget renderTarget;
		private Texture texture;
		private Shader shader;

		public RaindropTester()
		{
			grid = new vec3[Width, Height];
			renderTarget = new RenderTarget(Width, Height, RenderTargetFlags.Color);
			texture = new Texture();

			shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "Sprite.vert");
			shader.Attach(ShaderTypes.Fragment, "Raindrops.frag");
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<byte>(4, GL_UNSIGNED_BYTE, false, true);
			shader.CreateProgram();
		}

		public void Update(float dt)
		{
		}

		public void Draw(SpriteBatch sb)
		{
		}
	}
}
