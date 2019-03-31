using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using static Engine.GL;
using static Engine.GLFW;

namespace Zeldo
{
	public class MainGame : Game
	{
		public MainGame() : base("Zeldo")
		{
			glClearColor(0, 0, 0, 1);
		}

		protected override void Update(float dt)
		{
		}

		protected override void Draw()
		{
			glClear(GL_COLOR_BUFFER_BIT);
		}
	}
}
