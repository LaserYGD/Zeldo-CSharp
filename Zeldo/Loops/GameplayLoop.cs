using System.Collections.Generic;
using System.Linq;
using Engine.Core._3D;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Physics;
using Engine.Sensors;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using Jitter;
using Jitter.Collision;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Zeldo.Entities;
using Zeldo.Entities.Core;
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

		// TODO: Use properties.
		public const int PhysicsIterations = 8;
		public const int Gravity = -18;

		private Camera3D camera;
		private World world;
		private Scene scene;
		private Space space;

		// Visualizers.
		private SpaceVisualizer spaceVisualizer;
		private JitterVisualizer jitterVisualizer;

		private bool isFrameAdvanceEnabled;
		private bool isFrameAdvanceReady;

		public GameplayLoop(TitleLoop titleLoop = null) : base(LoopTypes.Gameplay)
		{
			// TODO: Pull relevant objects from the title loop (likely camera and maybe scene).
			camera = new Camera3D();
		}

		public override void Initialize()
		{
			CollisionSystem system = new CollisionSystemSAP();
			system.UseTriangleMeshNormal = true;
			system.CollisionDetected += OnCollision;

			var body = new RigidBody(TriangleMeshLoader.Load("Physics/Triangle_Physics.obj"));
			body.IsStatic = true;

			// TODO: Should damping factors be left in their default states? (they were changed while adding kinematic bodies)
			world = new World(system);
			world.Gravity = new JVector(0, Gravity, 0);
			world.SetDampingFactors(1, 1);
			world.AddBody(body);

			space = new Space();
			scene = new Scene
			{
				Camera = camera,
				Canvas = canvas,
				Space = space,
				World = world
			};

			canvas.Clear();
			canvas.Load("Hud.json");
			canvas.GetElement<DebugView>().IsVisible = false;

			// TODO: Set player position from a save slot.
			Player player = new Player();
			player.Position = new vec3(2, 3, -1);
			player.UnlockSkill(PlayerSkills.Grab);
			player.UnlockSkill(PlayerSkills.Jump);
			player.UnlockSkill(PlayerSkills.Ascend);

			// TODO: Load fragments from a save slot.
			scene.Add(player);

			// TODO: Load settings from a file.
			ControlSettings settings = new ControlSettings();
			settings.MouseSensitivity = 50;

			camera.Attach(new FollowController(player, settings));

			var sprite = new Sprite3D("Link.png");
			sprite.Position = new vec3(1, 2.5f, -3);
			sprite.Scale = new vec2(1.5f);

			// TODO: Initialize renderer settings from a configuration file (based on user settings).
			// TODO: Set light color and direction based on time of day and weather.
			var renderer = scene.Renderer;
			renderer.Light.Direction = Utilities.Normalize(new vec3(2f, -0.35f, -2.5f));
			renderer.Add(new Model("Triangle.obj"));
			renderer.Add(sprite);
			renderTargetUsers3D.Add(renderer);

			// Create visualizers.
			spaceVisualizer = new SpaceVisualizer(camera, space);
			jitterVisualizer = new JitterVisualizer(camera, world);

			MessageSystem.Subscribe(this, CoreMessageTypes.Keyboard, (messageType, data, dt) =>
			{
				ProcessKeyboard((KeyboardData)data);
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }

		private void OnCollision(RigidBody body1, RigidBody body2, JVector point1, JVector point2, JVector normal,
			JVector[] triangle, float penetration)
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
			if (!isFrameAdvanceEnabled || isFrameAdvanceReady)
			{
				world.Step(dt, true, PhysicsStep, PhysicsIterations);
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
