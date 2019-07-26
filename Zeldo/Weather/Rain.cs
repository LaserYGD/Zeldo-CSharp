using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;
using Engine.Shaders;
using static Engine.GL;

namespace Zeldo.Weather
{
	public class Rain : WeatherFormation
	{
		private Shader shader;
		private GLBuffer buffer;

		public Rain()
		{
			buffer = new GLBuffer(2000, 200);

			shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "Rain.vert");
			shader.Attach(ShaderTypes.Geometry, "Rain.geom");
			shader.Attach(ShaderTypes.Fragment, "Rain.frag");
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.Initialize();
		}

		public override void Dispose()
		{
		}

		public override void Update(float dt)
		{
		}
	}
}
