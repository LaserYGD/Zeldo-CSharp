using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics;
using Engine.Shapes._3D;
using Engine.UI;
using Engine.View;
using GlmSharp;
using static Engine.GL;

namespace Tycoon
{
	public class MainGame : Game
	{
		private Camera3D camera;
		private Canvas canvas;
		private Sprite sprite;
		private SpriteText text;
		private SpriteBatch sb;
		private Box box;
		private PrimitiveRenderer3D primitives3D;

		public MainGame() : base("Tycoon")
		{
			glClearColor(0, 0, 0, 1);
			glEnable(GL_BLEND);
			glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
			glPrimitiveRestartIndex(65535);

			camera = new Camera3D();
			//camera.IsOrthographic = true;
			canvas = new Canvas();
			sprite = new Sprite("Link.png");
			sprite.Position = new vec2(0, 50);
			text = new SpriteText("Default", "Good vs. Evil Minecraft :)");
			text.Position = new vec2(220, 20);
			sb = new SpriteBatch();
			box = new Box(1, 2, 3);
			box.Position = new vec3(0, 0, 4);
			primitives3D = new PrimitiveRenderer3D();

			// Setting window dimensions also sends out a Resize message.
			Resolution.WindowDimensions = new ivec2(800, 600);
		}

		protected override void Update(float dt)
		{
			camera.Update(dt);
		}

		protected override void Draw()
		{
			primitives3D.Draw(box, Color.White);
			primitives3D.Flush(camera);

			/*
			ivec2 dimensions = Resolution.WindowDimensions;

			glClear(GL_COLOR_BUFFER_BIT);
			glViewport(0, 0,(uint)dimensions.x, (uint)dimensions.y);
			glDepthFunc(GL_NEVER);

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

			sprite.Draw(sb);
			sprite.Position += new vec2(1, 0.5f);
			sprite.Rotation -= 0.02f;
			sprite.Scale -= new vec2(0.001f);

			for (int i = 0; i < colors.Length; i++)
			{
				sb.DrawLine(new ivec2(100, 100 + i * 25), new ivec2(300, 200 + i * 25), colors[i]);
			}

			text.Draw(sb);
			text.Position += new vec2(0.1f, 0);

			canvas.Draw(sb);
			sb.Flush();
			*/
		}
	}
}
