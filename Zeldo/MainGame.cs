using System.Collections.Generic;
using Engine;
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
using Engine.Sensors;
using Engine.UI;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using Jitter.LinearMath;
using Zeldo.Entities;
using Zeldo.Settings;
using Zeldo.State;
using Zeldo.UI;
using Zeldo.View;
using static Engine.GL;
using static Engine.GLFW;

namespace Zeldo
{
	public class MainGame : Game, IReceiver
	{
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
		private List<IRenderTargetUser3D> renderTargetUsers;
		private PrimitiveRenderer3D primitives;

		private SpaceVisualizer spaceVisualizer;
		private JitterVisualizer jitterVisualizer;
		private TentacleTester tentacleTester;

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
			sb = new SpriteBatch();

			mainTarget = new RenderTarget(Resolution.RenderWidth, Resolution.RenderHeight, RenderTargetFlags.Color |
				RenderTargetFlags.Depth);
			mainSprite = new Sprite(mainTarget, null, Alignments.Left | Alignments.Top);
			mainSprite.Mods = SpriteModifiers.FlipVertical;

			canvas = new Canvas();
			canvas.Load("Hud.json");
			canvas.GetElement<DebugView>().IsVisible = false;

			spaceVisualizer = new SpaceVisualizer(camera, space);

			MasterRenderer3D renderer = scene.Renderer;
			renderer.Light.Direction = Utilities.Normalize(new vec3(2f, -0.35f, -2.5f));

			Player player = new Player();
			player.Position = CreateDemoCubes ? new vec3(2, 3, -3.5f) : new vec3(2, 3, -2);
			player.UnlockSkill(PlayerSkills.Grab);
			player.UnlockSkill(PlayerSkills.Jump);

			ControlSettings settings = new ControlSettings();
			settings.MouseSensitivity = 50;

			camera.Attach(new FollowController(player, settings));

			scene.Add(player);
			//scene.LoadFragment("Demo.json");

			// The tentacle testing is intentionally created after the player is added to the scene.
			tentacleTester = new TentacleTester(scene);

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
				tentacleTester.Update(dt);
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
