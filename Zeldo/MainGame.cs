using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using Engine.Core;
using Engine.Core._2D;
using Engine.Entities;
using Engine.Graphics;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Shapes._3D;
using Engine.UI;
using Engine.View;
using GlmSharp;
using Zeldo.Entities;
using Zeldo.Entities.Enemies;
using Zeldo.UI.Hud;
using static Engine.GL;

namespace Zeldo
{
	public class MainGame : Game, IReceiver
	{
		private SpriteBatch sb;
		private RenderTarget mainTarget;
		private Sprite mainSprite;
		private SpriteText attackText;
		private Camera3D camera;
		private Canvas canvas;
		private Scene scene;
		private Player player;
		private Skeleton skeleton;
		private PrimitiveRenderer3D primitives;

		public MainGame() : base("Zeldo")
		{
			glClearColor(0, 0, 0, 1);
			glEnable(GL_BLEND);
			glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
			glPrimitiveRestartIndex(65535);

			camera = new Camera3D();
			camera.IsOrthographic = true;
			camera.Orientation *= quat.FromAxisAngle(0.75f, vec3.UnitX);
			camera.Position = new vec3(0, 0, 3) * camera.Orientation;

			sb = new SpriteBatch();
			mainTarget = new RenderTarget(Resolution.RenderWidth, Resolution.RenderHeight,
				RenderTargetFlags.Color | RenderTargetFlags.Depth);
			mainSprite = new Sprite(mainTarget, Alignments.Left | Alignments.Top);
			mainSprite.Mods = SpriteModifiers.FlipVertical;

			PlayerHealthDisplay healthDisplay = new PlayerHealthDisplay();
			PlayerManaDisplay manaDisplay = new PlayerManaDisplay();

			attackText = new SpriteText("Default");
			attackText.Position = new vec2(20);

			canvas = new Canvas();
			canvas.Add(healthDisplay);
			canvas.Add(manaDisplay);

			player = new Player
			{
				HealthDisplay = healthDisplay,
				ManaDisplay = manaDisplay
			};

			skeleton = new Skeleton();
			skeleton.Position = new vec3(-1.5f, 0, 1);

			scene = new Scene(camera);
			scene.Add(player);
			scene.Add(skeleton);

			primitives = new PrimitiveRenderer3D();
			
			MessageSystem.Subscribe(this, CoreMessageTypes.ResizeWindow, (messageType, data, dt) => { OnResize(); });

			// Calling this function here is required to ensure that all classes receive initial resize messages.
			MessageSystem.ProcessChanges();
			MessageSystem.Send(CoreMessageTypes.ResizeRender, Resolution.RenderDimensions);
			MessageSystem.Send(CoreMessageTypes.ResizeWindow, Resolution.WindowDimensions);
		}

		public List<MessageHandle> MessageHandles { get; set; }

		private void OnResize()
		{
			mainSprite.ScaleTo(Resolution.WindowWidth, Resolution.WindowHeight);
		}

		protected override void Update(float dt)
		{
			scene.Update(dt);
			camera.Update(dt);
		}

		protected override void Draw()
		{
			glEnable(GL_CULL_FACE);

			mainTarget.Apply();
			//scene.Draw(camera);
			primitives.Draw(player.Box, Color.White);
			primitives.Draw(skeleton.Box, Color.Red);
			primitives.Flush(camera);

			glBindFramebuffer(GL_FRAMEBUFFER, 0);
			glClear(GL_COLOR_BUFFER_BIT);
			glViewport(0, 0, (uint)Resolution.WindowWidth, (uint)Resolution.WindowHeight);
			glDepthFunc(GL_NEVER);
			glDisable(GL_CULL_FACE);

			mainSprite.Draw(sb);
			canvas.Draw(sb);
			attackText.Value = $"{player.AttackLine.P1.x:N2}, {player.AttackLine.P1.y:N2}";
			attackText.Draw(sb);
			sb.DrawLine(player.AttackLine, Color.Yellow);
			sb.Flush();
		}
	}
}
