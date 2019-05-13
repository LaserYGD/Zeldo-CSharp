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
using Zeldo.Entities.Core;
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

		/*
		private List<RigidBody> debugBodies = new List<RigidBody>();
		private List<Model> debugModels = new List<Model>();
		private RepeatingTimer timer;
		
		private int cubeCount = 0;
		*/

		private SkeletalTester skeletalTester;
		//private CharacterControlTester tester;

		public MainGame() : base("Zeldo")
		{
			glClearColor(0, 0, 0, 1);
			glEnable(GL_BLEND);
			glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
			glPrimitiveRestartIndex(Constants.RestartIndex);

			Properties.Load("Entity.properties");

			camera = new Camera3D();
			//camera.IsOrthographic = true;
			camera.Orientation *= quat.FromAxisAngle(0.75f, vec3.UnitX);
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

			/*
			Player player = new Player
			{
				HealthDisplay = healthDisplay,
				ManaDisplay = manaDisplay
			};

			player.UnlockSkill(PlayerSkills.Jump);
			*/

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
			batch.Add(new Model("Triangle.dae"));
			batch.LightDirection = Utilities.Normalize(new vec3(1, -0.5f, -0.25f));

			//scene.Add(player);

			renderTargetUsers = new List<IRenderTargetUser>();
			renderTargetUsers.Add(scene.ModelBatch);

			//LoadTestingData();

			skeletalTester = new SkeletalTester();
			//tester = new CharacterControlTester();

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

		private void LoadTestingData()
		{
			TriangleMeshShape shape = TriangleMeshLoader.Load("MapPhysics.obj");
			RigidBody body = new RigidBody(shape);
			body.IsStatic = true;

			world.AddBody(body);

			Model model = new Model("Map");

			ModelBatch batch = scene.ModelBatch;
			batch.Add(model);
			batch.LightDirection = Utilities.Normalize(new vec3(1, -0.5f, -0.5f));

			/*
			const int TotalCubes = 120;

			timer = new RepeatingTimer(time =>
			{
				Random random = new Random();

				const float Range = 3.5f;
				const float AngularVelocityRange = 1;

				float x = (float)random.NextDouble() * Range * 2 - Range;
				float y = 6;
				float z = (float)random.NextDouble() * Range * 2 - Range;
				float sizeX = 0.5f;
				float sizeY = 0.5f;
				float sizeZ = 0.5f;
				float angleX = (float)random.NextDouble() * Constants.TwoPi;
				float angleY = (float)random.NextDouble() * Constants.TwoPi;
				float angleZ = (float)random.NextDouble() * Constants.TwoPi;
				float angularVelocityX = (float)random.NextDouble() * AngularVelocityRange;
				float angularVelocityY = (float)random.NextDouble() * AngularVelocityRange;
				float angularVelocityZ = (float)random.NextDouble() * AngularVelocityRange;

				quat orientation = quat.FromAxisAngle(angleX, vec3.UnitX) *
				                   quat.FromAxisAngle(angleY, vec3.UnitY) * quat.FromAxisAngle(angleZ, vec3.UnitZ);

				RigidBody box = new RigidBody(new BoxShape(sizeX, sizeY, sizeZ));
				box.Position = new JVector(x, y, z);
				box.Orientation = orientation.ToJMatrix();
				box.AngularVelocity = new JVector(angularVelocityX, angularVelocityY, angularVelocityZ);

				world.AddBody(box);

				Model cube = new Model("Cube");
				cube.Scale = new vec3(sizeX, sizeY, sizeZ);
				cube.Position = box.Position.ToVec3();
				cube.Orientation = box.Orientation.ToQuat();

				batch.Add(cube);

				debugBodies.Add(box);
				debugModels.Add(cube);

				return ++cubeCount < TotalCubes;
			}, 0.01f);

			timer.Paused = true;

			MessageSystem.Subscribe(this, CoreMessageTypes.Mouse, (messageType, data, dt) =>
			{
				if (timer.Complete)
				{
					return;
				}

				MouseData mouseData = (MouseData)data;

				if (mouseData.Query(GLFW.GLFW_MOUSE_BUTTON_LEFT, InputStates.PressedThisFrame))
				{
					timer.Paused = false;
				}
			});
			*/
		}

		protected override void Update(float dt)
		{
			//world.Step(dt, true, PhysicsStep, 8);
			//space.Update();
			//scene.Update(dt);
			camera.Update(dt);

			/*
			if (!timer.Complete)
			{
				timer.Update(dt);
			}

			for (int i = 0; i < debugBodies.Count; i++)
			{
				var body = debugBodies[i];
				var model = debugModels[i];

				model.Position = body.Position.ToVec3();
				model.Orientation = body.Orientation.ToQuat();
			}
			*/

			//tester.Update(dt);
			skeletalTester.Update(dt);

			MessageSystem.ProcessChanges();
		}

		protected override void Draw()
		{
			glEnable(GL_DEPTH_TEST);
			glEnable(GL_CULL_FACE);
			glDepthFunc(GL_LEQUAL);
			
			//renderTargetUsers.ForEach(u => u.DrawTargets());
			skeletalTester.DrawTargets();
			mainTarget.Apply();
			//scene.ModelBatch.Draw(camera);
			skeletalTester.Draw(camera);

			glBindFramebuffer(GL_FRAMEBUFFER, 0);
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
			glViewport(0, 0, (uint)Resolution.WindowWidth, (uint)Resolution.WindowHeight);
			glDisable(GL_DEPTH_TEST);
			glDisable(GL_CULL_FACE);
			glDepthFunc(GL_NEVER);

			mainSprite.Draw(sb);		
			//canvas.Draw(sb);
			//tester.Draw(sb);
			
			sb.Flush();
		}
	}
}
