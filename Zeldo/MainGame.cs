using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Shaders;
using Engine.Shapes._2D;
using Engine.Shapes._3D;
using Engine.UI;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using Zeldo.Entities;
using Zeldo.Entities.Core;
using Zeldo.Entities.Enemies;
using Zeldo.Sensors;
using Zeldo.UI.Hud;
using Zeldo.UI.Screens;
using static Engine.GL;

namespace Zeldo
{
	public class MainGame : Game, IReceiver
	{
		private SpriteBatch sb;
		private RenderTarget mainTarget;
		private Sprite mainSprite;
		private Camera3D camera;
		private Canvas canvas;
		private Scene scene;
		private Space space;
		private Player player;
		private Skeleton skeleton;
		private InventoryScreen inventoryScreen;
		private PrimitiveRenderer3D primitives;
		private ScreenManager screenManager;

		private JumpTester jumpTester;
		private JumpTester2 jumpTester2;
		private ModelTester modelTester;
		private Sprite shadowSprite;
		private Shader shadowShader;

		private SpriteText attackText;
		private SpriteText healthText;

		public MainGame() : base("Zeldo")
		{
			glClearColor(0, 0, 0, 1);
			glEnable(GL_BLEND);
			glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
			glPrimitiveRestartIndex(Constants.RestartIndex);

			camera = new Camera3D();
			camera.IsOrthographic = true;
			camera.Orientation *= quat.FromAxisAngle(0.75f, vec3.UnitX);
			camera.Position = new vec3(0, 0, 3) * camera.Orientation;

			sb = new SpriteBatch();
			mainTarget = new RenderTarget(Resolution.RenderWidth, Resolution.RenderHeight,
				RenderTargetFlags.Color | RenderTargetFlags.Depth);
			mainSprite = new Sprite(mainTarget, null, Alignments.Left | Alignments.Top);
			mainSprite.Mods = SpriteModifiers.FlipVertical;

			PlayerHealthDisplay healthDisplay = new PlayerHealthDisplay();
			PlayerManaDisplay manaDisplay = new PlayerManaDisplay();

			canvas = new Canvas();
			canvas.Add(healthDisplay);
			canvas.Add(manaDisplay);

			screenManager = new ScreenManager();
			screenManager.Load(canvas);

			space = new Space();

			player = new Player
			{
				HealthDisplay = healthDisplay,
				ManaDisplay = manaDisplay
			};

			player.UnlockSkill(PlayerSkills.Jump);

			skeleton = new Skeleton();
			skeleton.Position = new vec3(-1.5f, 0, 1);

			scene = new Scene
			{
				Camera = camera,
				Space = space
			};

			scene.Add(player);
			scene.Add(skeleton);

			primitives = new PrimitiveRenderer3D(camera);
			jumpTester = new JumpTester();
			jumpTester2 = new JumpTester2();
			modelTester = new ModelTester(camera);
			
			shadowShader = new Shader();
			shadowShader.Attach(ShaderTypes.Vertex, "Sprite.vert");
			shadowShader.Attach(ShaderTypes.Fragment, "ShadowMapVisualization.frag");
			shadowShader.AddAttribute<float>(2, GL_FLOAT);
			shadowShader.AddAttribute<float>(2, GL_FLOAT);
			shadowShader.AddAttribute<float>(4, GL_UNSIGNED_BYTE, true);
			shadowShader.CreateProgram();

			shadowSprite = new Sprite(modelTester.ShadowTarget, null, Alignments.Left | Alignments.Bottom);
			shadowSprite.Position = new vec2(0, 600);
			shadowSprite.ScaleTo(256, 256);
			shadowSprite.Shader = shadowShader;

			inventoryScreen = new InventoryScreen();
			inventoryScreen.Location = new ivec2(400, 300);

			attackText = new SpriteText("Default");
			healthText = new SpriteText("Default");

			SpriteText[] array =
			{
				attackText,
				healthText
			};

			for (int i = 0; i < array.Length; i++)
			{
				SpriteText text = array[i];
				text.Position = new vec2(20, 20 * (i + 1));
				text.Color = Color.Magenta;
			}

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
			space.Update();
			camera.Update(dt);

			//jumpTester.Update(dt);
			//jumpTester2.Update(dt);
			modelTester.Update(dt);

			attackText.Value = $"Attack: {player.AttackString}";
			healthText.Value = $"Enemy health: {skeleton.Health}/{skeleton.MaxHealth}";

			MessageSystem.ProcessChanges();
		}

		protected override void Draw()
		{
			glEnable(GL_DEPTH_TEST);
			glEnable(GL_CULL_FACE);
			glDepthFunc(GL_LEQUAL);

			modelTester.DrawTargets();
			mainTarget.Apply();
			modelTester.Draw(camera);

			/*
			var sensor = skeleton.Sensor;
			var swordSensor = player.SwordSensor;
			
			//scene.Draw(camera);
			primitives.Draw(player.Box, Color.White);
			primitives.Draw(skeleton.Box, Color.Red);
			primitives.Draw((Circle)sensor.Shape, sensor.Elevation, Color.Cyan, 20);

			if (swordSensor.Enabled)
			{
				primitives.Draw((Arc)swordSensor.Shape, swordSensor.Elevation, Color.Cyan, 10);
			}
			*/

			vec3 p1 = vec3.UnitY * 1;
			vec3 p2 = p1 + modelTester.LightDirection * 1.5f;

			primitives.DrawLine(p1, p2, Color.Yellow);
			primitives.Flush();

			glBindFramebuffer(GL_FRAMEBUFFER, 0);
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
			glViewport(0, 0, (uint)Resolution.WindowWidth, (uint)Resolution.WindowHeight);
			glDisable(GL_DEPTH_TEST);
			glDisable(GL_CULL_FACE);
			glDepthFunc(GL_NEVER);

			mainSprite.Draw(sb);
			//attackText.Draw(sb);
			//healthText.Draw(sb);

			shadowSprite.Draw(sb);
			//canvas.Draw(sb);
			//jumpTester.Draw(sb);
			//jumpTester2.Draw(sb);

			sb.Flush();
		}
	}
}
