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
using Zeldo.Entities;
using static Engine.GL;

namespace Zeldo
{
	public class MainGame : Game
	{
		private Camera3D camera;
		private Canvas canvas;
		private Sprite sprite;
		private SpriteText text;
		private SpriteBatch sb;
		private Box box;
		private Player player;
		private PrimitiveRenderer3D primitives3D;

		public MainGame() : base("Zeldo")
		{
			glClearColor(0, 0, 0, 1);
			glEnable(GL_BLEND);
			glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
			glPrimitiveRestartIndex(65535);

			camera = new Camera3D();
			//camera.IsOrthographic = true;
			camera.Orientation *= quat.FromAxisAngle(1, vec3.UnitX);
			camera.Position = new vec3(0, 0, 3) * camera.Orientation;
			canvas = new Canvas();
			sprite = new Sprite("Link.png");
			sprite.Position = new vec2(0, 50);
			text = new SpriteText("Default", "Good vs. Evil Minecraft :)");
			text.Position = new vec2(220, 20);
			sb = new SpriteBatch();
			box = new Box(2);
			box.Orientation = quat.FromAxisAngle(20, vec3.UnitX);
			primitives3D = new PrimitiveRenderer3D();
			player = new Player();

			// Setting window dimensions also sends out a Resize message.
			Resolution.WindowDimensions = new ivec2(800, 600);
		}

		protected override void Update(float dt)
		{
			box.Orientation *= quat.FromAxisAngle(0.01f, vec3.UnitY);
			player.Update(dt);
			camera.Update(dt);
		}

		protected override void Draw()
		{
			ivec2 dimensions = Resolution.WindowDimensions;

			glClear(GL_COLOR_BUFFER_BIT);
			glViewport(0, 0, (uint)dimensions.x, (uint)dimensions.y);

			player.Draw(camera);

			return;

			vec3[] points =
			{
				new vec3(0, 0.5f, 0), 
				new vec3(-0.5f, -0.5f, 0), 
				new vec3(0.5f, -0.5f, 0) 
			};

			glClear(GL_COLOR_BUFFER_BIT);
			glViewport(0, 0,(uint)dimensions.x, (uint)dimensions.y);
			
			primitives3D.Draw(box, Color.White);
			//primitives3D.DrawTriangle(points, Color.White);
			primitives3D.Flush(camera);
			
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
		}
	}
}
