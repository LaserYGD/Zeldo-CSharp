using System;
using System.Collections.Generic;
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

		private static SurfaceTriangle triangle1;
		private static SurfaceTriangle triangle2;

		// This is temporary for run testing on arbitrary surfaces.
		public static SurfaceTriangle ActiveTriangle { get; private set; }
		public static vec3 SlopeDirection { get; private set; }
		public static float Y { get; private set; }

		static PlayerController()
		{
			triangle1 = new SurfaceTriangle(vec3.Zero, new vec3(0, 0.8f, -5), new vec3(5, 2, 0), 0,
				Windings.CounterClockwise);
			triangle2 = new SurfaceTriangle(new vec3(6, 1.5f, -4), new vec3(0, 0.8f, -5), new vec3(5, 2, 0), 0,
				Windings.Clockwise);
			ActiveTriangle = triangle1;
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
			bool forward = data.Query(controls.RunForward, InputStates.Held);
			bool back = data.Query(controls.RunBack, InputStates.Held);
			bool left = data.Query(controls.RunLeft, InputStates.Held);
			bool right = data.Query(controls.RunRight, InputStates.Held);

			// "Flat" direction means the direction the player would run on flat ground. The actual movement direction
			// depends on the current surface (specifically the normal).
			vec2 flatDirection = vec2.Zero;

			bool isAccelerating = false;

			if (forward ^ back)
			{
				flatDirection.y = forward ? 1 : -1;
				isAccelerating = true;
			}

			if (left ^ right)
			{
				flatDirection.x = left ? 1 : -1;
				isAccelerating = true;
			}

			if (!isAccelerating)
			{
				player.Velocity = vec3.Zero;
			}

			if ((forward ^ back) && (left ^ right))
			{
				flatDirection *= SqrtTwo;
			}

			flatDirection = Utilities.Rotate(flatDirection, FollowController.Yaw);

			var normal = ActiveTriangle.Normal;

			// This means the ground is completely flat (meaning that the flat direction can be used for movement
			// directly).
			if (normal.y == 1)
			{
			}

			vec2 flatNormal = Utilities.Normalize(normal.swizzle.xz);

			float d = Utilities.Dot(flatDirection, flatNormal);

			Y = -ActiveTriangle.Slope * d;
			SlopeDirection = Utilities.Normalize(new vec3(flatDirection.x, Y, flatDirection.y));

			vec3 v = player.Velocity;
			v += SlopeDirection * player.RunAcceleration * dt;

			float max = player.RunMaxSpeed;

			if (Utilities.LengthSquared(v) > max * max)
			{
				v = Utilities.Normalize(v) * max;
			}

			player.Velocity = v;

			vec3 p = player.Position + v * dt;

			if (ActiveTriangle.Project(p, out vec3 result))
			{
				player.Position = result;

				return;
			}

			var results = PhysicsUtilities.Raycast(player.Scene.World3D, p + normal * 0.01f, -normal, 1);

			if (results?.Body.Shape is TriangleMeshShape)
			{
				vec3 hit = results.Position;

				ActiveTriangle = hit.x - hit.z > 5 ? triangle2 : triangle1;
				ActiveTriangle.Project(results.Position, out result);

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
