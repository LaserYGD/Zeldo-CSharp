using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using Engine.Core._2D;
using Engine.Graphics;
using GlmSharp;
using static Engine.GL;

namespace Tycoon
{
	public class MainGame : Game
	{
		private Sprite sprite;
		private SpriteBatch sb;

		public MainGame() : base("Tycoon")
		{
			glClearColor(0, 0, 0, 1);

			sprite = new Sprite("Link.png");
			sb = new SpriteBatch();

			// Setting window dimensions also sends out a Resize message.
			Resolution.WindowDimensions = new ivec2(800, 600);
		}

		protected override void Update(float dt)
		{
		}

		protected override void Draw()
		{
			glClear(GL_COLOR_BUFFER_BIT);

			sprite.Draw(sb);
			sb.Flush();
		}
	}
}
