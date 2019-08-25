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
using Jitter.LinearMath;
using Zeldo.Entities.Weapons;
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

		// This is temporary for run testing on arbitrary surfaces.
		public static SurfaceTriangle ActiveSurface { get; private set; }
		public static vec3 SlopeDirection { get; private set; }

		private Player player;
		private PlayerData playerData;
		private PlayerControls controls;

		// Attacks use a short input buffering window in order to make chained attacks easier to execute.
		private SingleTimer attackBuffer;

		// Player jumping has variable height, meaning that releasing the bind early cuts your jump short. Since the
		// player can have multiple jump binds, though, this limit should only apply if the SAME bind was released.
		private InputBind jumpBindUsed;

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
			// TODO: Consider using the SetSurfacePosition function instead (would require passing delta time) to make sure the kinematic body always works.
			player.Position = result + new vec3(0, player.Height / 2, 0);
			//player.SetSurfacePosition(result);
			jumpBindUsed = null;
		}

		private void ProcessInput(FullInputData data, float dt)
		{
			ProcessRunning(data, dt);

			// Ascension reuses the jump bind (since it's conceptually also an "up" action). That means that if an
			// ascend was
			if (!ProcessAscend(data))
			{
				ProcessJumping(data);
			}

			ProcessAttack(data, dt);
			//ProcessInteraction(data);
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
				player.SetSurfacePosition(p);

				// This prevents very slow drift when standing still on sloped surfaces.
				return;
			}

			p += v * dt;

			// The player's position is always set at the bottom of this function. If this function return true, that
			// means the player is still within the current triangle.
			if (!ActiveSurface.Project(p, out vec3 result))
			{
				// TODO: Store a reference to the physics map separate (rather than querying the world every frame).
				var world = player.Scene.World;
				var map = world.RigidBodies.First(b => b.Shape is TriangleMeshShape);
				var normal = ActiveSurface.Normal;
				var results = PhysicsUtilities.Raycast(world, map, p + normal * 0.2f, -normal, 1);

				if (results?.Triangle != null)
				{
					ActiveSurface = new SurfaceTriangle(results.Triangle, results.Normal, 0);
					ActiveSurface.Project(results.Position, out result);
					
					player.OnSurfaceTransition(ActiveSurface);
				}
			}

			// Note that this behavior can result in the player's position being set just outside the current triangle
			// (even if the raycast didn't return another one). This is a failsafe to account for potential weird
			// behavior when very close to a triangle's edge. In theory, this failsafe shouldn't be needed (assuming a
			// seamlessly interconnected map without gaps in the geometry), but it's safer to keep it (plus the fix
			// shouldn't be tiny and unnoticeable anyway).
			player.SetSurfacePosition(result);
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

		private void ProcessJumping(FullInputData data)
		{
			if (!player.SkillsEnabled[JumpIndex])
			{
				return;
			}

			// If this is true, it's assumed that the jump bind must have been populated. Note that this case also
			// handles breaking from an ascend via a jump.
			if (player.State == PlayerStates.Jumping)
			{
				if (data.Query(jumpBindUsed, InputStates.ReleasedThisFrame))
				{
					player.LimitJump();
					jumpBindUsed = null;
				}

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
