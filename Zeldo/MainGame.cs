using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Graphics._3D;
using Engine.Graphics._3D.Rendering;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.Localization;
using Engine.Messaging;
using Engine.Physics;
using Engine.UI;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using Jitter;
using Jitter.Collision;
using Jitter.LinearMath;
using Zeldo.Control;
using Zeldo.Entities;
using Zeldo.Entities.Core;
using Zeldo.Entities.Weapons;
using Zeldo.Physics;
using Zeldo.Sensors;
using Zeldo.Settings;
using Zeldo.State;
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

		private Gamestates currentState;
		private Gamestates nextState;

		private SpriteBatch sb;
		private RenderTarget mainTarget;
		private Sprite mainSprite;
		private Camera3D camera;
		private Canvas canvas;
		private Scene scene;
		private Space space;
		private World world;
		private ScreenManager screenManager;
		private List<IRenderTargetUser3D> renderTargetUsers;
		private PrimitiveRenderer3D primitives;
		private SpriteText spriteText;

		private SpaceVisualizer spaceVisualizer;
		private JitterVisualizer jitterVisualizer;

		private GameLoop activeLoop;

		public MainGame() : base("Zeldo")
		{
			glClearColor(0, 0, 0, 1);
			glEnable(GL_BLEND);
			glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
			glPrimitiveRestartIndex(Constants.RestartIndex);
			glfwSetInputMode(window.Address, GLFW_CURSOR, GLFW_CURSOR_DISABLED);

			currentState = Gamestates.Unassigned;
			nextState = Gamestates.Splash;

			Properties.LoadAll();
			Language.Reload(Languages.English);

			camera = new Camera3D();
			primitives = new PrimitiveRenderer3D(camera, 10000, 1000);
			spriteText = new SpriteText("Debug");
			spriteText.Position = new vec2(500, 20);
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
			system.CollisionDetected += (body1, body2, point1, point2, normal, triangle, penetration) =>
			{
				// By design, all physics objects have entities attached, with the exception of static parts of the
				// map. In the case of map collisions, it's unknown which body comes first as an argument (as of
				// writing this comment, anyway), which is why both entities are checked for null.
				Entity entity1 = body1.Tag as Entity;
				Entity entity2 = body2.Tag as Entity;

				vec3 p1 = point1.ToVec3();
				vec3 p2 = point2.ToVec3();

				// The normal needs to be flipped based on how Jitter handles triangle winding.
				vec3 n = Utilities.Normalize(-normal.ToVec3());

				// A triangle will only be given in the case of collisions with the map.
				if (triangle != null)
				{
					var entity = entity1 ?? entity2;
					var point = entity1 != null ? p2 : p1;
					var tArray = triangle.Select(t => t.ToVec3()).ToArray();

					entity.OnCollision(point, n, tArray);

					return;
				}

				entity1?.OnCollision(entity2, p1, n);
				entity2?.OnCollision(entity1, p2, -n);
			};
			
			world = new World(system);
			space = new Space();
			spaceVisualizer = new SpaceVisualizer(camera, space);

			scene = new Scene
			{
				Camera = camera,
				Canvas = canvas,
				Space = space,
				World = world
			};

			scene.LoadFragment("Triangle.json");

			MasterRenderer3D renderer = scene.Renderer;
			renderer.Light.Direction = Utilities.Normalize(new vec3(2f, -0.35f, -0.7f));

			Bow bow = new Bow();
			bow.Initialize(scene, null);

			Player player = new Player
			{
				HealthDisplay = healthDisplay,
				ManaDisplay = manaDisplay,
				DebugView = debugView
			};

			player.Position = new vec3(2, 20, -2);
			//player.Attach(new RunController());

			player.UnlockSkill(PlayerSkills.Grab);
			player.UnlockSkill(PlayerSkills.Jump);
			player.Equip(bow);

			ControlSettings settings = new ControlSettings();
			settings.MouseSensitivity = 50;

			camera.Attach(new FollowController(player, settings));

			scene.Add(player);
			//scene.LoadFragment("Demo.json");

			renderTargetUsers = new List<IRenderTargetUser3D>();
			renderTargetUsers.Add(scene.Renderer);

			jitterVisualizer = new JitterVisualizer(camera, world);
			jitterVisualizer.IsEnabled = true;

			MessageSystem.Subscribe(this, CoreMessageTypes.Keyboard, (messageType, data, dt) =>
			{
				ProcessKeyboard((KeyboardData)data);
			});

			MessageSystem.Subscribe(this, CoreMessageTypes.ResizeWindow, (messageType, data, dt) =>
			{
				mainSprite.ScaleTo(Resolution.WindowWidth, Resolution.WindowHeight);
			});

			MessageSystem.Subscribe(this, CustomMessageTypes.Gamestate, (messageType, data, dt) =>
			{
				var state = (Gamestates)data;

				// This implementation means that if multiple gamestate messages are sent on a single frame, the last
				// state will take priority (although that should never happen in practice).
				if (state != currentState)
				{
					nextState = state;
				}
			});

			// Calling this function here is required to ensure that all classes receive initial resize messages.
			MessageSystem.ProcessChanges();
			MessageSystem.Send(CoreMessageTypes.ResizeRender, Resolution.RenderDimensions);
			MessageSystem.Send(CoreMessageTypes.ResizeWindow, Resolution.WindowDimensions);
		}

		public List<MessageHandle> MessageHandles { get; set; }

		private void ProcessKeyboard(KeyboardData data)
		{
			if (data.Query(GLFW_KEY_ESCAPE, InputStates.PressedThisFrame))
			{
				glfwSetWindowShouldClose(window.Address, 1);
			}

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
				scene.Renderer.IsEnabled = !scene.Renderer.IsEnabled;
			}
		}

		public void Dispose()
		{
			MessageSystem.Unsubscribe(this);
		}

		protected override void Update(float dt)
		{
			world.Step(dt, true, PhysicsStep, PhysicsMaxSteps);
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
			//spaceVisualizer.Draw();

			// This is temporary for run testing.
			/*
			var triangle = PlayerController.ActiveTriangle;
			var points = triangle.Points;
			var flatPoints = SurfaceTriangle.FlatPoints;

			vec3 center = vec3.Zero;

			for (int i = 0; i < 3; i++)
			{
				vec3 p1 = points[i];
				vec3 p2 = points[(i + 1) % 3];
				vec2 fp1 = flatPoints[i];
				vec2 fp2 = flatPoints[(i + 1) % 3];

				primitives.DrawLine(p1, p2, Color.Red);
				primitives.DrawLine(new vec3(fp1.x, 0, fp1.y), new vec3(fp2.x, 0, fp2.y), Color.Magenta);

				center += p1;
			}

			center /= 3;

			vec3 d1 = center + triangle.Normal;
			vec3 d2 = d1 + PlayerController.SlopeDirection;

			if (triangle.Project(d2, out vec3 result))
			{
				primitives.DrawLine(d2, result, Color.Yellow);
			}

			primitives.DrawLine(center, center + vec3.UnitY, Color.Blue);
			primitives.DrawLine(center, d1, Color.Cyan);
			primitives.DrawLine(d1, d1 + SurfaceTriangle.Axis, Color.Red);
			primitives.DrawLine(d1, d2, Color.Green);
			primitives.Flush();
			*/

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
			
			// This is temporary.
			spriteText.Draw(sb);

			sb.Flush();
		}
	}
}
