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

		private int physicsMaxIterations;

		public GameplayLoop(TitleLoop titleLoop = null) : base(LoopTypes.Gameplay)
		{
			// TODO: Pull relevant objects from the title loop (likely camera and maybe scene).
			camera = new Camera3D();
			physicsMaxIterations = Properties.GetInt("physics.max.iterations");
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
			player.Unlock(PlayerSkills.WallJump);
			player.Unlock(PlayerSkills.Ascend);

			// Combat skills.
			player.Unlock(PlayerSkills.Block);
			player.Unlock(PlayerSkills.Parry);

			// TODO: Load fragments from a save slot.
			scene.Add(player);

			//var fragment = scene.LoadFragment("Demo.json");
			var fragment = scene.LoadFragment("Windmill.json");
			player.Position = fragment.Origin + fragment.Spawn;

			CreateDebugCubes();

			camera.Attach(new FollowView(camera, player, settings));

			// TODO: Initialize renderer settings from a configuration file (based on user settings).
			// TODO: Set light color and direction based on time of day and weather.
			var renderer = scene.Renderer;
			renderer.Light.Direction = Utilities.Normalize(new vec3(2f, -0.6f, -2f));

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
		
		private void ProcessKeyboard(KeyboardData data)
		{
			// Process frame advance.
			if (isFrameAdvanceEnabled && data.Query(GLFW_KEY_F, InputStates.PressedThisFrame))
			{
				isFrameAdvanceReady = true;
			}

			// Process reload.
			if (data.Query(GLFW_KEY_R, InputStates.PressedThisFrame))
			{
				// TODO: Clear the space visualizer.
				scene.Reload();
				jitterVisualizer.Clear();
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

		private void CreateDebugCubes()
		{
			/*
			var cube = new DummyCube(RigidBodyTypes.Dynamic, true, new vec3(3, 1, 3));
			cube.Position = new vec3(5, 8, 5);

			scene.Add(cube);
			*/

			/*
			var random = new Random();

			for (int i = 0; i < 10; i++)
			{
				float x = (float)random.NextDouble() * 10 - 5;
				float y = (float)random.NextDouble() * 5 + 8;
				float z = (float)random.NextDouble() * 10 - 5;

				var cube = new DummyCube(RigidBodyTypes.Dynamic, true, new vec3(3, 0.5f, 3));
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

		public override void Update(float dt)
		{
			if (!isFrameAdvanceEnabled || isFrameAdvanceReady)
			{
				// TODO: Should physics be multithreaded? Doing so caused physics to become inconsistent across multiple runs.
				world.Step(dt, false, PhysicsStep, physicsMaxIterations);
				space.Update();
				scene.Update(dt);
			}

			camera.Update(dt);
		}

		public override void Draw()
		{
			var renderer = scene.Renderer;
			var list = canvas.GetElement<DebugView>().GetGroup("Scene");
			list.Add("Entities: " + scene.Size);
			list.Add("Bodies: " + scene.World.RigidBodies.Count);
			list.Add("Models: " + renderer.Models.Size);
			list.Add("Skeletons: " + renderer.Skeletons.Size);
			list.Add("Sprites: " + renderer.Sprites.Size);

			scene.Draw(camera);
			jitterVisualizer.Draw(camera);
			spaceVisualizer.Draw();
		}
	}
}
