using System;
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
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Zeldo.Control;
using Zeldo.Entities;
using Zeldo.Entities.Core;
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
		public const float PhysicsStep = 1f / 120;
		public const int PhysicsIterations = 8;
		public const int Gravity = -18;

		private const bool FrameAdvanceEnabled = false;

		// This is temporary for kinematic physics testing.
		private const bool CreateDemoCubes = false;

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

		private bool frameAdvanceReady;

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

				entity1?.OnCollision(entity2, p1, -n, penetration);
				entity2?.OnCollision(entity1, p2, n, penetration);
			};

			world = new World(system);
			world.Gravity = new JVector(0, Gravity, 0);
			
			// TODO: Should damping factors be left in their default states? (they were changed while adding kinematic bodies)
			world.SetDampingFactors(1, 1);
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

			// Create testing cubes.
			var seekers = new List<DummyCube>();

			if (CreateDemoCubes)
			{
				var dTarget1 = new DummyCube(RigidBodyTypes.Dynamic);
				dTarget1.Position = new vec3(0, 2.5f, 6.0f);

				var dTarget2 = new DummyCube(RigidBodyTypes.Dynamic);
				dTarget2.Position = new vec3(0, 2.5f, 4.5f);

				var kTarget1 = new DummyCube(RigidBodyTypes.Kinematic);
				kTarget1.Position = new vec3(0, 2.5f, 3.0f);

				var kTarget2 = new DummyCube(RigidBodyTypes.Kinematic);
				kTarget2.Position = new vec3(0, 2.5f, 1.5f);

				var sTarget1 = new DummyCube(RigidBodyTypes.Static);
				sTarget1.Position = new vec3(0, 2.5f, 0.0f);

				var sTarget2 = new DummyCube(RigidBodyTypes.Static);
				sTarget2.Position = new vec3(0, 2.5f, -1.5f);

				var dSeeker1 = new DummyCube(RigidBodyTypes.Dynamic);
				dSeeker1.Position = new vec3(8, 3.0f, 6.0f);

				var kSeeker1 = new DummyCube(RigidBodyTypes.Kinematic);
				kSeeker1.Position = new vec3(9, 3.0f, 4.5f);

				var dSeeker2 = new DummyCube(RigidBodyTypes.Dynamic);
				dSeeker2.Position = new vec3(10, 3.0f, 3.0f);

				var kSeeker2 = new DummyCube(RigidBodyTypes.Kinematic);
				kSeeker2.Position = new vec3(11, 3.0f, 1.5f);

				var dSeeker3 = new DummyCube(RigidBodyTypes.Dynamic);
				dSeeker3.Position = new vec3(12, 3.0f, 0.0f);

				var kSeeker3 = new DummyCube(RigidBodyTypes.Kinematic);
				kSeeker3.Position = new vec3(13, 3.0f, -1.5f);

				// Add target cubes.
				scene.Add(dTarget1);
				scene.Add(dTarget2);
				scene.Add(kTarget1);
				scene.Add(kTarget2);
				scene.Add(sTarget1);
				scene.Add(sTarget2);

				// Add seeker cubes.
				scene.Add(dSeeker1);
				scene.Add(kSeeker1);
				scene.Add(dSeeker2);
				scene.Add(kSeeker2);
				scene.Add(dSeeker3);
				scene.Add(kSeeker3);

				// Add seeker cubes to the temporary list.
				seekers.Add(dSeeker1);
				seekers.Add(kSeeker1);
				seekers.Add(dSeeker2);
				seekers.Add(kSeeker2);
				seekers.Add(dSeeker3);
				seekers.Add(kSeeker3);
			}
			else
			{
				const float XRange = 12;
				const float YRange = 4;
				const float ZRange = 12;

				Random random = new Random();

				for (int i = 0; i < 40; i++)
				{
					float x = (float)random.NextDouble() * XRange - XRange / 2;
					float y = (float)random.NextDouble() * YRange + 2;
					float z = (float)random.NextDouble() * ZRange - ZRange / 2;

					var cube = new DummyCube(RigidBodyTypes.Dynamic);
					cube.Position = new vec3(x, y, z);

					scene.Add(cube);
				}

				var staticCube1 = new DummyCube(RigidBodyTypes.Static);
				staticCube1.Position = new vec3(-1, 1.5f, 1);

				var staticCube2 = new DummyCube(RigidBodyTypes.Static);
				staticCube2.Position = new vec3(0, 1.5f, 0);

				scene.Add(staticCube1);
				scene.Add(staticCube2);
			}

			MasterRenderer3D renderer = scene.Renderer;
			renderer.Light.Direction = Utilities.Normalize(new vec3(2f, -0.35f, -0.7f));

			Player player = new Player
			{
				HealthDisplay = healthDisplay,
				ManaDisplay = manaDisplay,
				DebugView = debugView
			};

			player.Position = CreateDemoCubes ? new vec3(2, 3, -3.5f) : new vec3(2, 3, -2);
			player.UnlockSkill(PlayerSkills.Grab);
			player.UnlockSkill(PlayerSkills.Jump);

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

				if (CreateDemoCubes)
				{
					var kbData = (KeyboardData)data;

					if (kbData.Query(GLFW_KEY_P, InputStates.PressedThisFrame))
					{
						foreach (var seeker in seekers)
						{
							seeker.ControllingBody.LinearVelocity = new JVector(-2.5f, 0, 0);
						}
					}
				}
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
			// Process the game exiting.
			if (data.Query(GLFW_KEY_ESCAPE, InputStates.PressedThisFrame))
			{
				glfwSetWindowShouldClose(window.Address, 1);
			}

			// Process frame advance.
			if (data.Query(GLFW_KEY_F, InputStates.PressedThisFrame))
			{
				frameAdvanceReady = true;
			}

			bool controlHeld = data.Query(GLFW_KEY_LEFT_CONTROL, InputStates.Held) ||
				data.Query(GLFW_KEY_RIGHT_CONTROL, InputStates.Held);

			if (!controlHeld)
			{
				return;
			}

			// Toggle Jitter visualization.
			if (data.Query(GLFW_KEY_J, InputStates.PressedThisFrame))
			{
				jitterVisualizer.IsEnabled = !jitterVisualizer.IsEnabled;
			}

			// Toggle mesh visualization.
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
			if (!FrameAdvanceEnabled || frameAdvanceReady)
			{
				world.Step(dt, true, PhysicsStep, PhysicsIterations);
				space.Update();
				scene.Update(dt);
			}

			camera.Update(dt);
			frameAdvanceReady = false;

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
