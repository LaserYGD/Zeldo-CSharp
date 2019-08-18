using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Physics;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Zeldo.Physics;
using Zeldo.View;

namespace Zeldo.Entities
{
	public class PlayerController : IReceiver
	{
		private const int DashIndex = (int)PlayerSkills.Dash;
		private const int GrabIndex = (int)PlayerSkills.Grab;
		private const int JumpIndex = (int)PlayerSkills.Jump;

		// When moving using the keyboard, diagonal directions can be normalized by pre-computing this value (avoiding
		// an actual square root call at runtime).
		private const float SqrtTwo = 1.41421356237f;

		// This is temporary for run testing on arbitrary surfaces.
		public static SurfaceTriangle ActiveTriangle { get; private set; }
		public static vec3 SlopeDirection { get; private set; }

		static PlayerController()
		{
			/*
			triangle1 = new SurfaceTriangle(vec3.Zero, new vec3(0, 0.8f, -5), new vec3(5, 2, 0), 0,
				Windings.CounterClockwise);
			triangle2 = new SurfaceTriangle(new vec3(6, 1.5f, -4), new vec3(0, 0.8f, -5), new vec3(5, 2, 0), 0,
				Windings.Clockwise);
			ActiveTriangle = triangle1;
			*/
		}

		private Player player;
		private PlayerData playerData;
		private PlayerControls controls;

		public PlayerController(Player player, PlayerData playerData, PlayerControls controls)
		{
			this.player = player;
			this.playerData = playerData;
			this.controls = controls;

			MessageSystem.Subscribe(this, CoreMessageTypes.Input, (messageType, data, dt) =>
			{
				ProcessInput((FullInputData)data, dt);
			});
		}

		public FollowController FollowController { get; set; }
		public List<MessageHandle> MessageHandles { get; set; }

		public void Dispose()
		{
			MessageSystem.Unsubscribe(this);
		}

		private void ProcessInput(FullInputData data, float dt)
		{
			ProcessRunning(data, dt);
			//ProcessJumping(data);
			//ProcessInteraction(data);
		}

		/*
		private void ProcessAttack(FullInputData data)
		{
			if (data.Query(controls.Attack, InputStates.PressedThisFrame, out InputBind bindUsed))
			{
				vec2 direction = vec2.Zero;

				switch (bindUsed.InputType)
				{
					case InputTypes.Keyboard:
						break;

					case InputTypes.Mouse:
						MouseData mouseData = (MouseData)data.GetData(InputTypes.Mouse);

						vec4 projected = Scene.Camera.ViewProjection * new vec4(Position, 1);
						vec2 halfWindow = Resolution.WindowDimensions / 2;
						vec2 screenPosition = projected.swizzle.xy * halfWindow;

						screenPosition.y *= -1;
						screenPosition += halfWindow;
						direction = (mouseData.Location - screenPosition).Normalized;

						break;
				}

				float angle = Utilities.Angle(direction);

				//sword.Attack(direction);
				bow.PrimaryAttack(direction, angle);
			}
		}
		*/

		private void ProcessRunning(FullInputData data, float dt)
		{
			RaycastResults results;

			var world = player.Scene.World3D;
			var map = world.RigidBodies.First(b => b.Shape is TriangleMeshShape);

			vec3 p = player.Position;

			if (ActiveTriangle == null)
			{
				results = PhysicsUtilities.Raycast(world, map, player.Position, -vec3.UnitY, 10);
				ActiveTriangle = new SurfaceTriangle(results.Triangle, results.Normal, 0);
				p = results.Position;
			}

			bool forward = data.Query(controls.RunForward, InputStates.Held);
			bool back = data.Query(controls.RunBack, InputStates.Held);
			bool left = data.Query(controls.RunLeft, InputStates.Held);
			bool right = data.Query(controls.RunRight, InputStates.Held);

			// "Flat" direction means the direction the player would run on flat ground. The actual movement direction
			// depends on the current surface.
			vec2 flatDirection = vec2.Zero;

			if (forward ^ back)
			{
				flatDirection.y = forward ? 1 : -1;
			}

			if (left ^ right)
			{
				flatDirection.x = left ? 1 : -1;
			}

			if (flatDirection == vec2.Zero)
			{
				player.Velocity = vec3.Zero;
			}
			else if ((forward ^ back) && (left ^ right))
			{
				flatDirection *= SqrtTwo;
			}

			flatDirection = Utilities.Rotate(flatDirection, FollowController.Yaw);

			vec3 normal = ActiveTriangle.Normal;
			vec3 sloped;

			// This means the ground is completely flat (meaning that the flat direction can be used for movement
			// directly).
			if (normal.y == 1)
			{
				sloped = new vec3(flatDirection.x, 0, flatDirection.y);
			}
			else
			{
				vec2 flatNormal = Utilities.Normalize(normal.swizzle.xz);

				float d = Utilities.Dot(flatDirection, flatNormal);
				float y = -ActiveTriangle.Slope * d;

				sloped = Utilities.Normalize(new vec3(flatDirection.x, y, flatDirection.y));
			}

			vec3 v = player.Velocity;
			v += sloped * player.RunAcceleration * dt;

			// This is temporary (for visual debugging).
			SlopeDirection = sloped;

			float max = player.RunMaxSpeed;

			if (Utilities.LengthSquared(v) > max * max)
			{
				v = Utilities.Normalize(v) * max;
			}

			player.Velocity = v;
			p += v * dt;

			// This player is still within the triangle.
			if (ActiveTriangle.Project(p, out vec3 result))
			{
				player.Position = result;

				return;
			}

			results = PhysicsUtilities.Raycast(world, map, p + normal * 0.2f, -normal, 1);

			if (results?.Triangle != null)
			{
				ActiveTriangle = new SurfaceTriangle(results.Triangle, results.Normal, 0);
				ActiveTriangle.Project(results.Position, out result);

				player.Position = result;
			}
			else
			{
				// This is a failsafe to account for potential weird behavior when very close to a triangle's edge. In
				// theory, this case shouldn't be hit very often.
				player.Position = result;
			}
		}

		private void ProcessJumping(FullInputData data)
		{
			if (player.SkillsEnabled[JumpIndex] && data.Query(controls.Jump, InputStates.PressedThisFrame))
			{
				player.Jump();
			}
		}

		private void ProcessInteraction(FullInputData data)
		{
			if (data.Query(controls.Interact, InputStates.PressedThisFrame))
			{
				player.Interact();
			}
		}
	}
}
