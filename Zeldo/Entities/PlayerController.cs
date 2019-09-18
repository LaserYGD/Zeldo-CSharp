using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Input;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Physics;
using Engine.Timing;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Zeldo.Control;
using Zeldo.Physics;
using Zeldo.View;

namespace Zeldo.Entities
{
	public class PlayerController : IReceiver
	{
		private const int AscendIndex = (int)PlayerSkills.Ascend;
		private const int DashIndex = (int)PlayerSkills.Dash;
		private const int GrabIndex = (int)PlayerSkills.Grab;
		private const int JumpIndex = (int)PlayerSkills.Jump;

		// When moving using the keyboard, diagonal directions can be normalized by pre-computing this value (avoiding
		// an actual square root call at runtime).
		private const float SqrtTwo = 1.41421356237f;

		private Player player;
		private PlayerData playerData;
		private PlayerControls controls;
		private SurfaceTriangle surface;

		// Passing an array of controllers is easier than passing each one as a separate constructor argument.
		private AbstractController[] controllers;

		// Attacks use a short input buffering window in order to make chained attacks easier to execute.
		private SingleTimer attackBuffer;

		// It's possible for actions to have multiple binds. In cases where releasing a bind does something (e.g.
		// limiting a player's jump or releasing a hold), that action should only take place if the *same* bind was
		// released (rather than releasing a *different* button bound to the same action). In practice, then, that
		// means that while one bind is held in this scenario, other binds for that same action are ignored.
		private InputBind jumpBindUsed;
		private InputBind grabBindUsed;
		private InputBuffer grabBuffer;

		public PlayerController(Player player, PlayerData playerData, PlayerControls controls,
			AbstractController[] controllers)
		{
			this.player = player;
			this.playerData = playerData;
			this.controls = controls;
			this.controllers = controllers;

			attackBuffer = new SingleTimer(time => { });
			attackBuffer.IsRepeatable = true;
			attackBuffer.IsPaused = true;

			// Create buffers.
			float grab = Properties.GetFloat("player.grab.buffer");

			grabBuffer = new InputBuffer(grab, true, controls.Grab);

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

		public void OnLanding(SurfaceTriangle surface)
		{
			this.surface = surface;

			jumpBindUsed = null;
		}

		private void ProcessInput(FullInputData data, float dt)
		{
			vec2 flatDirection = ComputeFlatDirection(data);

			if (player.OnGround)
			{
				// TODO: Replace this with usage of the surface controller.
				ProcessRunning(data, dt);
			}
			else
			{
				((AerialController)controllers[Player.ControllerIndexes.Aerial]).FlatDirection = flatDirection;
			}

			// Ascension reuses the jump bind (since it's conceptually also an "up" action), but requires an additional
			// button to be held. By checking for ascension first, the jump input can be stored and reused for jump
			// processing even if that additional bind isn't held.
			if (!ProcessAscend(data))
			{
				ProcessJumping(data, dt);
			}

			ProcessAttack(data, dt);
			//ProcessInteraction(data);
		}

		private vec2 ComputeFlatDirection(FullInputData data)
		{
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
			
			return Utilities.Rotate(flatDirection, FollowController.Yaw);
		}
		
		private void ProcessRunning(FullInputData data, float dt)
		{
			// This assumes that if the player is grounded, an active surface will be set.
			if (!player.OnGround)
			{
				return;
			}

			vec3 halfHeight = new vec3(0, player.Height / 2, 0);
			vec3 p = player.Position - halfHeight;

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

			// TODO: Make a decision about whether to keep sliding in the game in some form.
			/*
			vec3 v = player.State == PlayerStates.Sliding
				? AdjustSlidingVelocity(flatDirection, dt)
				: AdjustRunningVelocity(flatDirection, dt);
			*/

			vec3 v = AdjustRunningVelocity(flatDirection, dt);
			player.SurfaceVelocity = v;

			if (v == vec3.Zero)
			{
				player.GroundPosition = p;

				// This prevents very slow drift when standing still on sloped surfaces.
				return;
			}

			p += v * dt;

			// The player's position is always set at the bottom of this function. If this function return true, that
			// means the player is still within the current triangle.
			if (!surface.Project(p, out vec3 result))
			{
				// TODO: Store a reference to the physics map separate (rather than querying the world every frame).
				var world = player.Scene.World;
				var map = world.RigidBodies.First(b => b.Shape is TriangleMeshShape);
				var normal = surface.Normal;
				var results = PhysicsUtilities.Raycast(world, map, p + normal * 0.2f, -normal, 1);

				if (results?.Triangle != null)
				{
					surface = new SurfaceTriangle(results.Triangle, results.Normal, 0);
					surface.Project(results.Position, out result);
					
					player.OnSurfaceTransition(surface);
				}
			}

			// Note that this behavior can result in the player's position being set just outside the current triangle
			// (even if the raycast didn't return another one). This is a failsafe to account for potential weird
			// behavior when very close to a triangle's edge. In theory, this failsafe shouldn't be needed (assuming a
			// seamlessly interconnected map without gaps in the geometry), but it's safer to keep it (plus the fix
			// shouldn't be tiny and unnoticeable anyway).
			player.GroundPosition = result;
		}

		private vec3 AdjustRunningVelocity(vec2 flatDirection, float dt)
		{
			vec3 v = player.SurfaceVelocity;

			// Decleration.
			if (flatDirection == vec2.Zero)
			{
				// The player is already stopped (and not accelerating).
				if (v == vec3.Zero)
				{
					return v;
				}

				// This assumes the player isn't moving exactly vertically (which shouldn't be possible since the
				// player can't run on walls).
				int oldSign = Math.Sign(v.x != 0 ? v.x : v.y);

				v -= Utilities.Normalize(v) * /*player.RunDeceleration*/ 50 * dt;

				int newSign = Math.Sign(v.x != 0 ? v.x : v.y);

				if (oldSign != newSign)
				{
					v = vec3.Zero;
				}

				return v;
			}

			// Accleration.
			flatDirection = Utilities.Rotate(flatDirection, FollowController.Yaw);

			vec3 normal = surface.Normal;
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
				float y = -surface.Slope * d;

				sloped = Utilities.Normalize(new vec3(flatDirection.x, y, flatDirection.y));
			}

			v += sloped * /*player.RunAcceleration*/ 60 * dt;

			float max = 10;//player.RunMaxSpeed;

			if (Utilities.LengthSquared(v) > max * max)
			{
				v = Utilities.Normalize(v) * max;
			}

			return v;

			return vec3.Zero;
		}

		private vec3 AdjustSlidingVelocity(vec2 flatDirection, float dt)
		{
			vec3 v = player.SurfaceVelocity;
			vec3 normal = surface.Normal;
			vec3 slideDirection = Utilities.Normalize(new vec3(normal.x, -surface.Slope, normal.z));

			return v;
		}
		
		private bool ProcessAscend(FullInputData data)
		{
			// There are two ascend-based actions the player can take: 1) starting an ascend (by holding the relevant
			// button and pressing jump), or 2) breaking out of an ongoing ascend (by pressing jump mid-ascend). Also
			// note that ascension is only enabled when the player is currently close enough to an ascend target (such
			// as a ladder or rope).
			if (!player.SkillsEnabled[AscendIndex] && player.State != PlayerStates.Ascending)
			{
				return false;
			}

			if (!data.Query(controls.Jump, InputStates.PressedThisFrame, out jumpBindUsed))
			{
				return false;
			}

			if (player.State != PlayerStates.Ascending)
			{
				player.Ascend();
			}
			else
			{
				player.BreakAscend();
			}

			return true;
		}

		private void ProcessGrab(FullInputData data, float dt)
		{
			if (grabBuffer.Refresh(data, dt, out grabBindUsed))
			{
			}
		}

		private void ProcessJumping(FullInputData data, float dt)
		{
			// If this is true, it's assumed that the jump bind must have been populated. Note that this case also
			// handles breaking from an ascend via a jump.
			if (player.State == PlayerStates.Jumping)
			{
				if (data.Query(jumpBindUsed, InputStates.ReleasedThisFrame) &&
				    player.ControllingBody.LinearVelocity.Y > playerData.JumpLimit)
				{
					player.LimitJump(dt);
					jumpBindUsed = null;
				}

				return;
			}

			if (!player.SkillsEnabled[JumpIndex])
			{
				return;
			}

			// The jump bind might have already been set while processing ascension input.
			if (jumpBindUsed != null || data.Query(controls.Jump, InputStates.PressedThisFrame, out jumpBindUsed))
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
			if (weapon.HasCooldownExpired(dt) && !attackBuffer.IsPaused)
			{
				TriggerPrimary();

				attackBuffer.Elapsed = 0;
				attackBuffer.IsPaused = true;

				return;
			}

			if (weapon.IsCoolingDown)
			{
				// The attack buffer is reset with each new input (assuming buffering was enabled by the weapon's
				// previous attack).
				if (attackBuffer.Duration > 0)
				{
					attackBuffer.Elapsed = 0;
					attackBuffer.IsPaused = false;
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
