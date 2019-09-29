﻿using System;
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
using Zeldo.Settings;
using Zeldo.View;

namespace Zeldo.Entities
{
	public class PlayerController : IReceiver
	{
		private const int AscendIndex = (int)PlayerSkills.Ascend;
		private const int DashIndex = (int)PlayerSkills.Dash;
		private const int GrabIndex = (int)PlayerSkills.Grab;
		private const int JumpIndex = (int)PlayerSkills.Jump;

		// Combat indexes.
		private const int BlockIndex = (int)PlayerSkills.Block;
		private const int ParryIndex = (int)PlayerSkills.Parry;

		// When moving using the keyboard, diagonal directions can be normalized by pre-computing this value (avoiding
		// an actual square root call at runtime).
		private const float SqrtTwo = 1.41421356237f;

		private Player player;
		private PlayerData playerData;
		private PlayerControls controls;
		private SurfaceTriangle surface;
		private ControlSettings settings;

		// TODO: Store controllers separately (to avoid lots of casting).
		// Passing an array of controllers is easier than passing each one as a separate constructor argument.
		private AbstractController[] controllers;
		private LadderController ladderController;

		// TODO: Modify to use input buffers (rather than manual timing).
		// Attacks use a short input buffering window in order to make chained attacks easier to execute.
		private SingleTimer attackBuffer;

		// It's possible for actions to have multiple binds. In cases where releasing a bind does something (e.g.
		// limiting a player's jump or releasing a hold), that action should only take place if the *same* bind was
		// released (rather than releasing a *different* button bound to the same action). In practice, then, that
		// means that while one bind is held in this scenario, other binds for that same action are ignored.
		private InputBind jumpBindUsed;
		private InputBind grabBindUsed;
		private InputBind blockBindUsed;

		private InputBuffer grabBuffer;
		private InputBuffer ascendBuffer;

		public PlayerController(Player player, PlayerData playerData, PlayerControls controls,
			ControlSettings settings, AbstractController[] controllers)
		{
			this.player = player;
			this.playerData = playerData;
			this.controls = controls;
			this.settings = settings;
			this.controllers = controllers;

			ladderController = (LadderController)controllers[Player.ControllerIndexes.Ladder];

			attackBuffer = new SingleTimer(time => { });
			attackBuffer.IsRepeatable = true;
			attackBuffer.IsPaused = true;

			// Create buffers.
			float grab = Properties.GetFloat("player.grab.buffer");
			float ascend = Properties.GetFloat("player.ascend.buffer");

			// Actual values for requiresHold on each buffer are set when control settings are applied.
			grabBuffer = new InputBuffer(grab, false, controls.Grab);
			ascendBuffer = new InputBuffer(ascend, false, controls.Jump);
			ascendBuffer.RequiredChords = controls.Ascend;

			settings.ApplyEvent += OnApply;

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

		private void OnApply(ControlSettings settings)
		{
			ascendBuffer.RequiresHold = !settings.UseToggleAscend;
			grabBuffer.RequiresHold = !settings.UseToggleGrab;
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
			else if (player.IsOnLadder)
			{
				ProcessLadder(data, dt);
			}
			else
			{
				((AerialController)controllers[Player.ControllerIndexes.Aerial]).FlatDirection = flatDirection;
			}

			// Ascension reuses the jump bind (since it's conceptually also an "up" action), but requires an additional
			// button to be held. By checking for ascension first, the jump input can be stored and reused for jump
			// processing even if that additional bind isn't held.
			if (!ProcessAscend(data, dt))
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

			// The player's position is always set at the bottom of this function. If the projection returns true, that
			// means the player is still within the current triangle.
			if (!surface.Project(p, out vec3 result))
			{
				// TODO: Store a reference to the physics map separately (rather than querying the world every frame).
				var world = player.Scene.World;
				var map = world.RigidBodies.First(b => b.Shape is TriangleMeshShape);
				var normal = surface.Normal;

				// The raycast needs to be offset upward enough to catch steps.
				var results = PhysicsUtilities.Raycast(world, map, p + normal, -normal, 1.2f);

				if (results?.Triangle != null)
				{
					surface = new SurfaceTriangle(results.Triangle, results.Normal, 0);
					surface.Project(results.Position, out result);
					
					player.OnSurfaceTransition(surface);
				}
				// If the player has moved past a surface triangle (without transitioning to another one), a very small
				// forgiveness distance is checked before signalling the player to become airborne. This distance is
				// small enough to not be noticeable during gameplay, but protects against potential floating-point
				// errors near the seams of triangles.
				else if (ComputeForgiveness(p, surface) > playerData.EdgeForgiveness)
				{
					player.BecomeAirborneFromLedge();
				}
			}

			// Note that this behavior can result in the player's position being set just outside the current triangle
			// (even if the raycast didn't return another one). This is a failsafe to account for potential weird
			// behavior when very close to a triangle's edge. In theory, this failsafe shouldn't be needed (assuming a
			// seamlessly interconnected map without gaps in the geometry), but it's safer to keep it (plus the fix
			// shouldn't be tiny and unnoticeable anyway).
			player.GroundPosition = result;
		}

		private float ComputeForgiveness(vec3 p, SurfaceTriangle surface)
		{
			// To compute the shortest distance to an edge of the triangle, points are rotated to a flat plane first
			// (using the surface normal).
			var q = Utilities.Orientation(surface.Normal, vec3.UnitY);
			var flatP = (q * p).swizzle.xz;
			var flatPoints = surface.Points.Select(v => (q * v).swizzle.xz).ToArray();
			var d = float.MaxValue;

			for (int i = 0; i < flatPoints.Length; i++)
			{
				var p1 = flatPoints[i];
				var p2 = flatPoints[(i + 1) % 3];

				d = Math.Min(d, Utilities.DistanceToLine(flatP, p1, p2));
			}

			return d;
		}

		private vec3 AdjustRunningVelocity(vec2 flatDirection, float dt)
		{
			vec3 v = player.SurfaceVelocity;

			// Deceleration.
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

			// Acceleration.
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

			float max = 6;//player.RunMaxSpeed;

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

		private void ProcessLadder(FullInputData data, float dt)
		{
			// Ladder climbing uses the same controls as running forward and back.
			bool up = data.Query(controls.RunForward, InputStates.Held);
			bool down = data.Query(controls.RunBack, InputStates.Held);

			ladderController.Direction = up ^ down ? (up ? 1 : -1) : 0;
			ladderController.Update(dt);
		}

		private bool ProcessAscend(FullInputData data, float dt)
		{
			// There are two ascend-based actions the player can take: 1) starting an ascend (by holding the relevant
			// button and pressing jump), or 2) breaking out of an ongoing ascend (by pressing jump mid-ascend). Also
			// note that ascension is only enabled when the player is currently close enough to an ascend target (such
			// as a ladder or rope).
			if (!player.SkillsEnabled[AscendIndex] && player.State != PlayerStates.Ascending)
			{
				return false;
			}

			if (!ascendBuffer.Refresh(data, dt))
			{
				return false;
			}

			if (player.State != PlayerStates.Ascending)
			{
				return player.TryAscend();
			}

			player.BreakAscend();

			return true;
		}

		private void ProcessGrab(FullInputData data, float dt)
		{
			// TODO: Player actions (in relation to state) will likely need to be refined. In this case, could other states prevent grabbing?
			if (player.State != PlayerStates.Grabbing)
			{
				if (grabBuffer.Refresh(data, dt, out grabBindUsed))
				{
					player.TryGrab();
				}

				return;
			}

			// Ladders are special among grabbable objects in that a toggle is always used to attach or detach from the
			// ladder (regardless of control settings). I've never played a game where you have to hold a button to
			// remain on a ladder.
			bool shouldRelease = settings.UseToggleGrab || player.IsOnLadder
				? data.Query(controls.Grab, InputStates.ReleasedThisFrame)
				: data.Query(grabBindUsed, InputStates.ReleasedThisFrame);

			if (shouldRelease)
			{
				player.ReleaseGrab();
			}
		}

		private void ProcessJumping(FullInputData data, float dt)
		{
			// If this is true, it's assumed that the jump bind must have been populated. Note that this case also
			// handles breaking from ascend via a jump.
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

			if (player.JumpsRemaining == 0)
			{
				return;
			}

			// The jump bind might have already been set while processing ascension input.
			if (data.Query(controls.Jump, InputStates.PressedThisFrame, out jumpBindUsed))
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

		private void ProcessBlock(FullInputData data)
		{
			var binds = controls.Block;

			if (settings.UseToggleBlock)
			{
				if (data.Query(binds, InputStates.PressedThisFrame))
				{
					if (player.IsBlocking)
					{
						player.Block();
					}
					else
					{
						player.Unblock();
					}
				}

				return;
			}

			if (!player.IsBlocking && data.Query(binds, InputStates.PressedThisFrame, out blockBindUsed))
			{
				player.Block();
			}
			else if (player.IsBlocking && data.Query(blockBindUsed, InputStates.ReleasedThisFrame))
			{
				player.Unblock();
			}
		}

		private void ProcessParry(FullInputData data)
		{
			// Parry will never be enabled (or unlocked) without the ability to block. A parrying tool (like a shield)
			// must be equipped as well. Once those conditions are satisfied, parrying works similarly to ascend, where
			// an existing bind (in this case, block) executes differently if chorded with the parry bind.
			if (!(player.SkillsEnabled[ParryIndex] && data.Query(controls.Parry, InputStates.Held)))
			{
				return;
			}

			// TODO: This means that the player needs to release block for at least a frame before parrying. Could instead have a dedicated parry button (but at time of writing, I prefer the chorded approach).
			if (data.Query(controls.Block, InputStates.PressedThisFrame))
			{
				player.Parry();
			}
		}

		private void ProcessInteraction(FullInputData data)
		{
			if (data.Query(controls.Interact, InputStates.PressedThisFrame))
			{
				player.TryInteract();
			}
		}
	}
}
