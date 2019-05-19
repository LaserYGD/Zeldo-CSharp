using System.Collections.Generic;
using Engine;
using Engine.Core;
using Engine.Core._2D;
using Engine.Core._3D;
using Engine.Graphics._2D;
using Engine.Graphics._3D;
using Engine.Interfaces;
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
using Zeldo.Entities;
using Zeldo.Entities.Core;
using Zeldo.Entities.Weapons;
using Zeldo.Physics;
using Zeldo.Sensors;
using Zeldo.UI.Hud;
using Zeldo.UI.Screens;
using static Engine.GL;

namespace Zeldo
{
	public class MainGame : Game, IReceiver
	{
		private const float PhysicsStep = 1 / 120f;

		private SpriteBatch sb;
		private RenderTarget mainTarget;
		private Sprite mainSprite;
		private Camera3D camera;
		private Canvas canvas;
		private Scene scene;
		private Space space;
		private World world;
		private ScreenManager screenManager;
		private List<IRenderTargetUser> renderTargetUsers;

		public MainGame() : base("Zeldo")
		{
			glClearColor(0, 0, 0, 1);
			glEnable(GL_BLEND);
			glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
			glPrimitiveRestartIndex(Constants.RestartIndex);

			Properties.Load("Entity.properties");

			camera = new Camera3D();
			camera.IsOrthographic = true;
			camera.Orientation *= quat.FromAxisAngle(0.8f, vec3.UnitX);
			camera.Position = new vec3(0, 0, 5) * camera.Orientation + new vec3(0, 0, -0.5f);

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

			CollisionSystem system = new CollisionSystemSAP();
			system.UseTriangleMeshNormal = true;
			system.CollisionDetected += (body1, body2, point1, point2, normal, penetration) =>
			{
				return;

				// This assumes that all physics bodies will have entities attached.
				Entity entity1 = (Entity)body1.Tag;
				Entity entity2 = (Entity)body2.Tag;

				vec3 p1 = point1.ToVec3();
				vec3 p2 = point2.ToVec3();
				vec3 n = normal.ToVec3();

				entity1.OnCollision(entity2, p1, n);
				entity2.OnCollision(entity1, p2, -n);
			};
			
			world = new World(system);
			space = new Space();
			
			scene = new Scene
			{
				Camera = camera,
				Canvas = canvas,
				Space = space,
				World = world
			};

			ModelBatch batch = scene.ModelBatch;
			batch.Add(new Model("Map.obj"));
			batch.LightDirection = Utilities.Normalize(new vec3(1, -0.5f, -0.25f));

			Bow bow = new Bow();
			bow.Initialize(scene);

			Player player = new Player
			{
				HealthDisplay = healthDisplay,
				ManaDisplay = manaDisplay
			};

			player.UnlockSkill(PlayerSkills.Jump);
			player.Equip(bow);

			scene.Add(player);

			renderTargetUsers = new List<IRenderTargetUser>();
			renderTargetUsers.Add(scene.ModelBatch);

			//LoadTestingData();

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

		public void Dispose()
		{
			MessageSystem.Unsubscribe(this);
		}

		protected override void Update(float dt)
		{
			//world.Step(dt, true, PhysicsStep, 8);
			//space.Update();
			scene.Update(dt);
			camera.Update(dt);

			MessageSystem.ProcessChanges();
		}

		protected override void Draw()
		{
			glEnable(GL_DEPTH_TEST);
			glEnable(GL_CULL_FACE);
			glDepthFunc(GL_LEQUAL);
			
			renderTargetUsers.ForEach(u => u.DrawTargets());
			mainTarget.Apply();
			scene.Draw(camera);

			glBindFramebuffer(GL_FRAMEBUFFER, 0);
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
			glViewport(0, 0, (uint)Resolution.WindowWidth, (uint)Resolution.WindowHeight);
			glDisable(GL_DEPTH_TEST);
			glDisable(GL_CULL_FACE);
			glDepthFunc(GL_NEVER);

			mainSprite.Draw(sb);		
			canvas.Draw(sb);
			
			sb.Flush();
		}
	}
}
