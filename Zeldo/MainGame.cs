using System;
using System.Collections.Generic;
using Engine;
using Engine.Core._2D;
using Engine.Core._3D;
using Engine.Graphics._2D;
using Engine.Graphics._3D;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.Localization;
using Engine.Messaging;
using Engine.Physics;
using Engine.Shapes._2D;
using Engine.UI;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using Jitter;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Zeldo.Controllers;
using Zeldo.Entities;
using Zeldo.Entities.Core;
using Zeldo.Entities.Objects;
using Zeldo.Entities.Weapons;
using Zeldo.Physics;
using Zeldo.Physics._2D;
using Zeldo.Sensors;
using Zeldo.UI;
using Zeldo.UI.Hud;
using Zeldo.UI.Screens;
using Zeldo.View;
using static Engine.GL;
using static Engine.GLFW;

namespace Zeldo
{
	public class MainGame : Game, IReceiver
	{
		private const float PhysicsStep = 1 / 120f;
		private const int PhysicsMaxSteps = 8;

		private SpriteBatch sb;
		private RenderTarget mainTarget;
		private Sprite mainSprite;
		private Camera3D camera;
		private Canvas canvas;
		private Scene scene;
		private Space space;
		private World world3D;
		private World2D world2D;
		private ScreenManager screenManager;
		private List<IRenderTargetUser3D> renderTargetUsers;

		private SpaceVisualizer spaceVisualizer;
		private JitterVisualizer jitterVisualizer;
		private StaircaseVisualizer staircaseVisualizer;

		public MainGame() : base("Zeldo")
		{
			glClearColor(0, 0, 0, 1);
			glEnable(GL_BLEND);
			glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
			glPrimitiveRestartIndex(Constants.RestartIndex);

			Properties.Load("Character.properties");
			Properties.Load("Enemy.properties");
			Properties.Load("Entity.properties");
			Properties.Load("Player.properties");
			Properties.Load("UI.properties");
			Properties.Load("View.properties");
			Properties.Load("World.properties");

			Language.Reload(Languages.English);

			camera = new Camera3D();
			camera.IsOrthographic = true;

			sb = new SpriteBatch();

			mainTarget = new RenderTarget(Resolution.RenderWidth, Resolution.RenderHeight,
				RenderTargetFlags.Color | RenderTargetFlags.Depth);
			mainSprite = new Sprite(mainTarget, null, Alignments.Left | Alignments.Top);
			mainSprite.Mods = SpriteModifiers.FlipVertical;

			PlayerHealthDisplay healthDisplay = new PlayerHealthDisplay();
			PlayerManaDisplay manaDisplay = new PlayerManaDisplay();
			PlayerDebugView debugView = new PlayerDebugView();

			canvas = new Canvas();
			canvas.Add(healthDisplay);
			canvas.Add(manaDisplay);
			canvas.Add(debugView);

			screenManager = new ScreenManager();
			screenManager.Load(canvas);		

			CollisionSystem system = new CollisionSystemSAP();
			system.UseTriangleMeshNormal = true;
			system.CollisionDetected += (body1, body2, point1, point2, normal, penetration) =>
			{
				// Most physics bodies will have entities attached. If null, it's assumed that the body corresponds to
				// a static part of the map.
				Entity entity1 = body1.Tag as Entity;
				Entity entity2 = body2.Tag as Entity;

				vec3 n = Utilities.Normalize(normal.ToVec3());

				entity1?.OnCollision(entity2, point1.ToVec3(), n);
				entity2?.OnCollision(entity1, point2.ToVec3(), -n);
			};
			
			world3D = new World(system);
			world2D = new World2D();
			space = new Space();
			spaceVisualizer = new SpaceVisualizer(camera, space);

			//canvas.Add(new GroundVisualizer(world2D));

			scene = new Scene
			{
				Camera = camera,
				Canvas = canvas,
				Space = space,
				World2D = world2D,
				World3D = world3D
			};

			ModelBatch batch = scene.ModelBatch;
			batch.LightDirection = Utilities.Normalize(new vec3(-0.25f, -0.35f, -0.7f));

			Bow bow = new Bow();
			bow.Initialize(scene);

			Player player = new Player
			{
				HealthDisplay = healthDisplay,
				ManaDisplay = manaDisplay,
				DebugView = debugView
			};

			const int StepCount = 25;

			const float InnerRadius = 3;
			const float OuterRadius = 8;
			const float StepHeight = 0.3f;
			const float StepSpread = Constants.Pi * 2 / StepCount;
			const float Height = StepCount * StepHeight;

			SpiralStaircase staircase = new SpiralStaircase(Height)
			{
				InnerRadius = InnerRadius,
				OuterRadius = OuterRadius,
				Slope =  StepHeight / StepSpread,
				IsClockwise = true,
			};

			staircase.Initialize(scene);
			staircase.SetTransform(new vec3(11, -Height, -6), Constants.PiOverTwo);
			staircaseVisualizer = new StaircaseVisualizer(camera, staircase, StepHeight, StepCount, StepSpread);

			player.UnlockSkill(PlayerSkills.Grab);
			player.UnlockSkill(PlayerSkills.Jump);
			player.Equip(bow);

			camera.Attach(new FollowCameraController(player));

			scene.Add(player);
			scene.LoadFragment("Demo.json");

			player.Position = new vec3(0, 20, 0);

			renderTargetUsers = new List<IRenderTargetUser3D>();
			renderTargetUsers.Add(scene.ModelBatch);

			jitterVisualizer = new JitterVisualizer(camera, world3D);

			MessageSystem.Subscribe(this, CoreMessageTypes.Keyboard, (messageType, data, dt) =>
			{
				ProcessKeyboard((KeyboardData)data);
			});

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

		private void ProcessKeyboard(KeyboardData data)
		{
			bool controlHeld = data.Query(GLFW_KEY_LEFT_CONTROL, InputStates.Held) ||
				data.Query(GLFW_KEY_RIGHT_CONTROL, InputStates.Held);

			if (!controlHeld)
			{
				return;
			}

			if (data.Query(GLFW_KEY_J, InputStates.PressedThisFrame))
			{
				jitterVisualizer.IsEnabled = !jitterVisualizer.IsEnabled;
			}

			if (data.Query(GLFW_KEY_M, InputStates.PressedThisFrame))
			{
				scene.ModelBatch.IsEnabled = !scene.ModelBatch.IsEnabled;
			}
		}

		public void Dispose()
		{
			MessageSystem.Unsubscribe(this);
		}

		protected override void Update(float dt)
		{
			world2D.Step(dt, PhysicsStep, PhysicsMaxSteps);
			world3D.Step(dt, true, PhysicsStep, PhysicsMaxSteps);
			space.Update();
			scene.Update(dt);
			camera.Update(dt);

			MessageSystem.ProcessChanges();
		}

		protected override void Draw()
		{
			// Render 3D targets.
			glEnable(GL_DEPTH_TEST);
			glEnable(GL_CULL_FACE);
			glDepthFunc(GL_LEQUAL);

			renderTargetUsers.ForEach(u => u.DrawTargets());
			mainTarget.Apply();
			scene.Draw(camera);
			jitterVisualizer.Draw(camera);
			staircaseVisualizer.Draw();
			spaceVisualizer.Draw();

			// Render 2D targets.
			glDisable(GL_DEPTH_TEST);
			glDisable(GL_CULL_FACE);
			glDepthFunc(GL_NEVER);

			canvas.DrawTargets(sb);

			// Draw to the main screen.
			glBindFramebuffer(GL_FRAMEBUFFER, 0);
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
			glViewport(0, 0, (uint)Resolution.WindowWidth, (uint)Resolution.WindowHeight);

			sb.ApplyTarget(null);
			mainSprite.Draw(sb);		
			canvas.Draw(sb);
			sb.Flush();
		}
	}
}
