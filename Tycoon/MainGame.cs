using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics;
using Engine.View;
using GlmSharp;
using static Engine.GL;

namespace Tycoon
{
	public class MainGame : Game
	{
		private Camera camera;
		private Sprite sprite;
		private SpriteBatch sb;

		public MainGame() : base("Tycoon")
		{
			glClearColor(0, 0, 0, 1);

			camera = new Camera();
			//sprite = new Sprite("Link.png");
			sb = new SpriteBatch();

			// Setting window dimensions also sends out a Resize message.
			Resolution.WindowDimensions = new ivec2(800, 600);
		}

		protected override void Update(float dt)
		{
			camera.Update(dt);
		}

		protected override void Draw()
		{
			glClear(GL_COLOR_BUFFER_BIT);

			Color[] colors =
			{
				Color.White,
				Color.Red,
				Color.Green,
				Color.Blue,
				Color.Yellow,
				Color.Cyan,
				Color.Magenta
			};

			//sprite.Draw(sb);

			for (int i = 0; i < colors.Length; i++)
			{
				sb.DrawLine(new ivec2(100, 100 + i * 25), new ivec2(300, 200 + i * 25), colors[i]);
			}

			sb.Flush();
		}
	}
}
