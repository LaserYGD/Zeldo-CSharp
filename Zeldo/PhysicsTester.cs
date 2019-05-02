using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core._3D;
using Engine.Graphics._3D;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.Messaging;
using Engine.Physics;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using Jitter;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using static Engine.GLFW;

namespace Zeldo
{
	public class PhysicsTester : IDynamic, IReceiver
	{
		private Model[] models;
		private RigidBody[] bodies;

		private World world;
		private RigidBody staticBody;
		private Model staticModel;
		private ModelBatch modelBatch;

		private bool enabled;

		public PhysicsTester()
		{
			const float StaticWidth = 20;
			const float StaticHeight = 0.5f;
			const float StaticDepth = 20;

			models = new Model[20];
			bodies = new RigidBody[models.Length];

			for (int i = 0; i < models.Length; i++)
			{
				float width = (i + 1) * 0.08f + 0.5f;
				float height = (i + 1) * 0.09f + 0.5f;
				float depth = (i + 1) * 0.1f + 0.5f;

				RigidBody body = new RigidBody(new BoxShape(width, height, depth));
				body.Position = new JVector(0, i * 5 + 10, 0);
				body.Orientation = JMatrix.CreateFromYawPitchRoll(i * 0.1f, i * 0.2f, i * 0.3f);
				body.Material = new Material
				{
					Restitution = 0.18f,
					StaticFriction = 0.9f
				};

				bodies[i] = body;

				Model model = new Model("Cube");
				model.Position = body.Position.ToVec3();
				model.Scale = new vec3(width, height, depth);
				models[i] = model;
			}

			staticBody = new RigidBody(new BoxShape(StaticWidth, StaticHeight, StaticDepth));
			staticBody.Position = new JVector(0, -1, 0);
			staticBody.IsStatic = true;
			staticModel = new Model("Cube");
			staticModel.Position = staticBody.Position.ToVec3();
			staticModel.Scale = new vec3(StaticWidth, StaticHeight, StaticDepth);
			modelBatch = new ModelBatch(100000, 10000);
			modelBatch.Add(staticModel);
			modelBatch.LightDirection = Utilities.Normalize(new vec3(-1, -0.2f, -0.5f));

			foreach (Model model in models)
			{
				modelBatch.Add(model);
			}

			world = new World(new CollisionSystemSAP());
			world.AddBody(staticBody);

			foreach (RigidBody body in bodies)
			{
				world.AddBody(body);
			}

			MessageSystem.Subscribe(this, CoreMessageTypes.Keyboard, (messageType, data, dt) =>
			{
				KeyboardData keyboardData = (KeyboardData)data;

				if (keyboardData.Query(GLFW_KEY_SPACE, InputStates.PressedThisFrame))
				{
					enabled = true;
				}
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }
		public ModelBatch Batch => modelBatch;

		public void Update(float dt)
		{
			if (!enabled)
			{
				return;
			}

			world.Step(dt, true);

			for (int i = 0; i < bodies.Length; i++)
			{
				RigidBody body = bodies[i];
				Model model = models[i];
				model.Position = body.Position.ToVec3();
				model.Orientation = body.Orientation.ToQuat();
			}
		}
	}
}
