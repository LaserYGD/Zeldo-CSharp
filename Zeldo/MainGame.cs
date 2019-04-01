using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using Engine.Core._2D;
using Engine.Graphics;
using static Engine.GL;

namespace Zeldo
{
	public class MainGame : Game
	{
		private Sprite sprite;
		private SpriteBatch sb;

		public MainGame() : base("Zeldo")
		{
			glClearColor(0, 0, 0, 1);

			sprite = new Sprite("Link.png");
			sb = new SpriteBatch();
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
