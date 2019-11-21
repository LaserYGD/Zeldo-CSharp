using System.Collections.Generic;
using System.Diagnostics;
using Engine;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Localization;
using Engine.Messaging;
using Engine.Props;
using Engine.UI;
using Engine.Utility;
using Zeldo.Loops;
using static Engine.GL;
using static Engine.GLFW;

namespace Zeldo
{
	public class MainGame : Game
	{
		private Gamestates currentState;
		private Gamestates nextState;

		private Canvas canvas;
		private SpriteBatch sb;
		private RenderTarget mainTarget;
		private Sprite mainSprite;
		private GameLoop activeLoop;

		public MainGame() : base("Zeldo")
		{
			glClearColor(0, 0, 0, 1);
			glEnable(GL_BLEND);
			glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
			glPrimitiveRestartIndex(Constants.RestartIndex);
			glfwSetInputMode(window.Address, GLFW_CURSOR, GLFW_CURSOR_DISABLED);

			Language.Reload(Languages.English);

			canvas = new Canvas();
			sb = new SpriteBatch();

			mainTarget = new RenderTarget(Resolution.RenderWidth, Resolution.RenderHeight, RenderTargetFlags.Color |
				RenderTargetFlags.Depth);
			mainSprite = new Sprite(mainTarget, null, Alignments.Left | Alignments.Top);
			mainSprite.Mods = SpriteModifiers.FlipVertical;

			// The first loop is created manually. Others are created via gamestate messages.
			currentState = Gamestates.Gameplay;
			nextState = currentState;
			activeLoop = CreateLoop(currentState);

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

				Debug.Assert(state != Gamestates.Splash, "Can't transition to the splash loop.");

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

		private void ProcessKeyboard(KeyboardData data)
		{
			// Process the game exiting.
			if (data.Query(GLFW_KEY_ESCAPE, InputStates.PressedThisFrame))
			{
				OnExit();
			}
		}

		protected override void Update(float dt)
		{
			activeLoop.Update(dt);
			canvas.Update(dt);

			// Gamestate changes don't apply until the end of the current frame.
			if (nextState != currentState)
			{
				activeLoop = CreateLoop(nextState);
				currentState = nextState;
			}

			MessageSystem.ProcessChanges();
		}

		private GameLoop CreateLoop(Gamestates state)
		{
			GameLoop loop = null;

			switch (state)
			{
				case Gamestates.Gameplay: loop = new GameplayLoop((TitleLoop)activeLoop);
					break;

				case Gamestates.Title: loop = new TitleLoop();
					break;

				case Gamestates.Splash: loop = new SplashLoop();
					break;
			}

			loop.Canvas = canvas;
			loop.SpriteBatch = sb;
			loop.Initialize();

			return loop;
		}

		protected override void Draw()
		{
			// Render 3D targets.
			glEnable(GL_DEPTH_TEST);
			glEnable(GL_CULL_FACE);
			glDepthFunc(GL_LEQUAL);

			activeLoop.DrawTargets();
			mainTarget.Apply();
			activeLoop.Draw();

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
