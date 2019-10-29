using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Core._2D;
using Engine.Core._3D;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Physics;
using Engine.Sensors;
using Engine.Timing;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using Jitter;
using Jitter.Collision;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Zeldo.Entities;
using Zeldo.Entities.Core;
using Zeldo.Entities.Player;
using Zeldo.Entities.Weapons;
using Zeldo.Physics;
using Zeldo.Settings;
using Zeldo.UI;
using Zeldo.View;
using static Engine.GLFW;

namespace Zeldo.Loops
{
	public class GameplayLoop : GameLoop, IReceiver
	{
		public const float PhysicsStep = 1f / 120;

		private Camera3D camera;
		private World world;
		private Scene scene;
		private Space space;

		// Visualizers.
		private SpaceVisualizer spaceVisualizer;
		private JitterVisualizer jitterVisualizer;

		private bool isFrameAdvanceEnabled;
		private bool isFrameAdvanceReady;

		private Sprite3D sprite;
		private RepeatingTimer cubeTimer;

		private int physicsMaxIterations;
		private int cubeCount;

		public GameplayLoop(TitleLoop titleLoop = null) : base(LoopTypes.Gameplay)
		{
			// TODO: Pull relevant objects from the title loop (likely camera and maybe scene).
			camera = new Camera3D();
			physicsMaxIterations = Properties.GetInt("physics.max.iterations");

			cubeTimer = new RepeatingTimer(progress =>
			{
				var cube = new DummyCube(RigidBodyTypes.Dynamic, true);
				cube.Position = scene.GetEntities<PlayerCharacter>(EntityGroups.Player)[0].Position +
					new vec3(0.1f, 5, 0.6f);

				scene.Add(cube);
				cubeCount++;

				return cubeCount < 25;
			}, 0.5f, 0.5f);
		}

		public override void Initialize()
		{
			CollisionSystem system = new CollisionSystemSAP();
			system.UseTriangleMeshNormal = true;

			// TODO: Should damping factors be left in their default states? (they were changed while adding kinematic bodies)
			world = new World(system);
			world.Gravity = new JVector(0, -PhysicsConstants.Gravity, 0);
			world.SetDampingFactors(1, 1);
			world.Events.ContactCreated += OnContact;

			space = new Space();
			scene = new Scene
			{
				Camera = camera,
				Canvas = canvas,
				Space = space,
				World = world
			};

			var stats = new StatisticsDisplay();
			stats.Anchor = Alignments.Left | Alignments.Top;
			stats.Offset = new ivec2(10);
			stats.IsVisible = false;

			var debug = new DebugView();
			debug.Anchor = Alignments.Left | Alignments.Top;
			debug.Offset = new ivec2(10);
			//debug.IsVisible = false;

			canvas.Clear();
			canvas.Load("Hud.json");
			canvas.Add(stats);
			canvas.Add(debug);
			//canvas.Add(new RopeTester());

			// TODO: Load settings from a file.
			ControlSettings settings = new ControlSettings();
			settings.MouseSensitivity = 50;

			// TODO: Set player position from a save slot.
			PlayerCharacter player = new PlayerCharacter(settings);
			player.Equip(new Sword(player));
			player.Unlock(PlayerSkills.Grab);
			player.Unlock(PlayerSkills.Jump);
			player.Unlock(PlayerSkills.DoubleJump);
			player.Unlock(PlayerSkills.Ascend);

			// Combat skills.
			player.Unlock(PlayerSkills.Block);
			player.Unlock(PlayerSkills.Parry);

			var platform1 = new MovingPlatform(new vec3(3, 0.5f, 3), new vec3(0, 3, 0), new vec3(0, 3, 16), 3);
			var platform2 = new MovingPlatform(new vec3(3, 0.5f, 3), new vec3(5, 2, 12), new vec3(5, 12, 12), 2);

			// TODO: Load fragments from a save slot.
			scene.Add(platform1);
			scene.Add(platform2);
			scene.Add(player);

			var fragment = scene.LoadFragment("Demo.json");
			player.Position = fragment.Origin + fragment.Spawn;

			CreateDebugCubes();

			camera.Attach(new FollowView(camera, player, settings));

			sprite = new Sprite3D("Link.png");
			sprite.Position = new vec3(0, 2.5f, -1);
			sprite.Scale = new vec2(1.5f);

			// TODO: Initialize renderer settings from a configuration file (based on user settings).
			// TODO: Set light color and direction based on time of day and weather.
			var renderer = scene.Renderer;
			renderer.Light.Direction = Utilities.Normalize(new vec3(2f, -0.6f, -2f));
			renderer.Add(sprite);

			renderTargetUsers3D.Add(renderer);

			// Create visualizers.
			spaceVisualizer = new SpaceVisualizer(camera, space);
			jitterVisualizer = new JitterVisualizer(camera, world);
			jitterVisualizer.IsEnabled = true;

			MessageSystem.Subscribe(this, CoreMessageTypes.Keyboard, (messageType, data, dt) =>
			{
				ProcessKeyboard((KeyboardData)data);
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }

		private void CreateDebugCubes()
		{
			/*
			var cube = new DummyCube(RigidBodyTypes.Dynamic, false);
			cube.Position = new vec3(0, 3.5f, 10);

			scene.Add(cube);
			*/

			/*
			var random = new Random();

			for (int i = 0; i < 20; i++)
			{
				float x = (float)random.NextDouble() * 10 - 5 + 10;
				float y = (float)random.NextDouble() * 5 + 5;
				float z = (float)random.NextDouble() * 10 - 5;

				var cube = new DummyCube(RigidBodyTypes.Dynamic, true);
				cube.Position = new vec3(x, y, z);

				scene.Add(cube);
			}
			*/
		}

		private bool OnContact(RigidBody body1, RigidBody body2, JVector point1, JVector point2, JVector normal,
			JVector[] triangle, float penetration)
		{
			// By design, all physics objects have entities attached, with the exception of static parts of the map. In
			// the case of map collisions, it's unknown which body comes first as an argument (as of writing this
			// comment, anyway), which is why both entities are checked for null.
			Entity entity1 = body1.Tag as Entity;
			Entity entity2 = body2.Tag as Entity;

			vec3 p1 = point1.ToVec3();
			vec3 p2 = point2.ToVec3();

			// The normal needs to be flipped based on how Jitter handles triangle winding.
			//vec3 n = Utilities.Normalize(-normal.ToVec3());
			vec3 n = Utilities.Normalize(normal.ToVec3());

			// A triangle will only be given in the case of collisions with a triangle mesh (or terrain).
			if (triangle != null)
			{
				var entity = entity1 ?? entity2;
				var point = entity1 != null ? p2 : p1;
				var tArray = triangle.Select(t => t.ToVec3()).ToArray();

				return entity.OnContact(point, n, tArray, penetration);
			}

			bool b1 = entity1?.OnContact(entity2, body2, p1, -n, penetration) ?? true;
			bool b2 = entity2?.OnContact(entity1, body1, p2, n, penetration) ?? true;

			// Either entity can negate the contact.
			return b1 && b2;
		}

		public override void Dispose()
		{
			MessageSystem.Unsubscribe(this);
		}

		private void ProcessKeyboard(KeyboardData data)
		{
			// Process frame advance.
			if (isFrameAdvanceEnabled && data.Query(GLFW_KEY_F, InputStates.PressedThisFrame))
			{
				isFrameAdvanceReady = true;
			}

			bool controlHeld = data.Query(GLFW_KEY_LEFT_CONTROL, InputStates.Held) ||
				data.Query(GLFW_KEY_RIGHT_CONTROL, InputStates.Held);

			if (!controlHeld)
			{
				return;
			}

			// Toggle frame advance mode.
			if (data.Query(GLFW_KEY_F, InputStates.PressedThisFrame))
			{
				isFrameAdvanceEnabled = !isFrameAdvanceEnabled;
			}

			// Toggle Jitter visualization.
			if (data.Query(GLFW_KEY_J, InputStates.PressedThisFrame))
			{
				jitterVisualizer.IsEnabled = !jitterVisualizer.IsEnabled;
			}

			// Toggle the scene's master renderer (meshes, skeletons, and 3D sprites).
			if (data.Query(GLFW_KEY_M, InputStates.PressedThisFrame))
			{
				var renderer = scene.Renderer;
				renderer.IsEnabled = !renderer.IsEnabled;
			}
		}

		public override void Update(float dt)
		{
			//cubeTimer.Update(dt);

			if (!isFrameAdvanceEnabled || isFrameAdvanceReady)
			{
				sprite.Orientation *= quat.FromAxisAngle(dt, vec3.UnitY);

				// TODO: Should physics be multithreaded? Doing so caused physics to become inconsistent across multiple runs.
				world.Step(dt, false, PhysicsStep, physicsMaxIterations);
				space.Update();
				scene.Update(dt);
			}

			camera.Update(dt);
		}

		public override void Draw()
		{
			scene.Draw(camera);
			jitterVisualizer.Draw(camera);
			spaceVisualizer.Draw();
		}
	}
}
