using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Physics;
using Engine.Timing;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Zeldo.Entities.Weapons;
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
		public static SurfaceTriangle ActiveSurface { get; private set; }
		public static vec3 SlopeDirection { get; private set; }

		private Player player;
		private PlayerData playerData;
		private PlayerControls controls;

		// Attacks use a short input buffering window in order to make chained attacks easier to execute.
		private SingleTimer attackBuffer;

		public PlayerController(Player player, PlayerData playerData, PlayerControls controls)
		{
			this.player = player;
			this.playerData = playerData;
			this.controls = controls;

			attackBuffer = new SingleTimer(time => { });
			attackBuffer.Repeatable = true;
			attackBuffer.Paused = true;

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

		public void OnLanding(vec3 p, SurfaceTriangle triangle)
		{
			ActiveSurface = triangle;
			ActiveSurface.Project(p, out vec3 result);

			// The player's onGround flag is set to true before this function is called.
			player.Position = result;
		}

		private void ProcessInput(FullInputData data, float dt)
		{
			ProcessRunning(data, dt);
			//ProcessJumping(data);
			ProcessAttack(data, dt);
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
			// TODO: While airborne, a different controller should be used, meaning that this check shouldn't be needed (since the controller will only be enabled while the player is grounded).
			if (ActiveSurface == null)
			{
				return;
			}

			vec3 p = player.Position;

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

			// This normalizes the velocity when moving diagonally using a keyboard.
			if ((forward ^ back) && (left ^ right))
			{
				flatDirection *= SqrtTwo;
			}

			vec3 v = player.IsSliding
				? AdjustSlidingVelocity(flatDirection, dt)
				: AdjustRunningVelocity(flatDirection, dt);

			player.SurfaceVelocity = v;
			p += v * dt;

			// This player is still within the triangle.
			if (ActiveSurface.Project(p, out vec3 result))
			{
				player.Position = result;

				return;
			}

			// TODO: Store a reference to the physics map separate (rather than querying the world every frame).
			var world = player.Scene.World;
			var map = world.RigidBodies.First(b => b.Shape is TriangleMeshShape);
			var normal = ActiveSurface.Normal;
			var results = PhysicsUtilities.Raycast(world, map, p + normal * 0.2f, -normal, 1);

			if (results?.Triangle != null)
			{
				ActiveSurface = new SurfaceTriangle(results.Triangle, results.Normal, 0);
				ActiveSurface.Project(results.Position, out result);

				player.Position = result;
				player.OnSurfaceTransition(ActiveSurface);
			}
			else
			{
				// This is a failsafe to account for potential weird behavior when very close to a triangle's edge. In
				// theory, this case shouldn't be hit very often (assuming a seamlessly interconnected map without gaps
				// in the geometry).
				player.Position = result;
			}
		}

		private vec3 AdjustRunningVelocity(vec2 flatDirection, float dt)
		{
			vec3 v = player.SurfaceVelocity;

			// Decleration.
			if (flatDirection == vec2.Zero)
			{
				// This assumes the player isn't moving exactly vertically (which shouldn't be possible since the
				// player can't run on walls).
				int oldSign = Math.Sign(v.x != 0 ? v.x : v.y);

				v -= Utilities.Normalize(v) * player.RunDeceleration * dt;

				int newSign = Math.Sign(v.x != 0 ? v.x : v.y);

				if (oldSign != newSign)
				{
					v = vec3.Zero;
				}

				return v;
			}

			// Accleration.
			flatDirection = Utilities.Rotate(flatDirection, FollowController.Yaw);

			vec3 normal = ActiveSurface.Normal;
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
				float y = -ActiveSurface.Slope * d;

				sloped = Utilities.Normalize(new vec3(flatDirection.x, y, flatDirection.y));
			}

			v += sloped * player.RunAcceleration * dt;

			// This is temporary (for visual debugging).
			SlopeDirection = sloped;

			float max = player.RunMaxSpeed;

			if (Utilities.LengthSquared(v) > max * max)
			{
				v = Utilities.Normalize(v) * max;
			}

			return v;
		}

		private vec3 AdjustSlidingVelocity(vec2 flatDirection, float dt)
		{
			vec3 v = player.SurfaceVelocity;
			vec3 normal = ActiveSurface.Normal;
			vec3 slideDirection = Utilities.Normalize(new vec3(normal.x, -ActiveSurface.Slope, normal.z));

			return v;
		}

		private void ProcessJumping(FullInputData data)
		{
			if (player.SkillsEnabled[JumpIndex] && data.Query(controls.Jump, InputStates.PressedThisFrame))
			{
				player.Jump();
			}
		}

		private void ProcessAttack(FullInputData data, float dt)
		{
			var weapon = player.Weapon;

			// Helper function to trigger a weapon's primary attack with buffer time.
			void TriggerPrimary()
			{
				float bufferTime = weapon.TriggerPrimary();

				// A buffer time of zero means that no buffering should occur for that particular attack.
				if (bufferTime != 0)
				{
					// Triggering an attack doesn't also start the buffer timer. That only happens when *another*
					// attack input arrives while the weapon is on cooldown.
					attackBuffer.Duration = bufferTime;
				}
			}

			// This means that the player has no weapon equipped.
			if (weapon == null || !data.Query(controls.Attack, InputStates.PressedThisFrame))
			{
				return;
			}

			// If an attack was buffered as the weapon's cooldown expires, trigger another attack immediately.
			if (weapon.HasCooldownExpired(dt) && !attackBuffer.Paused)
			{
				TriggerPrimary();

				attackBuffer.Elapsed = 0;
				attackBuffer.Paused = true;

				return;
			}

			if (weapon.OnCooldown)
			{
				// The attack buffer is reset with each new input (assuming buffering was enabled by the weapon's
				// previous attack).
				if (attackBuffer.Duration > 0)
				{
					attackBuffer.Elapsed = 0;
					attackBuffer.Paused = false;
				}
			}
			else
			{
				TriggerPrimary();
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
