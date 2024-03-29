﻿using System.Collections.Generic;
using Engine;
using Engine.Input;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Props;
using Engine.Timing;
using Engine.Utility;
using GlmSharp;
using Zeldo.Combat;
using Zeldo.Control;
using Zeldo.Settings;
using Zeldo.View;

namespace Zeldo.Entities.Player
{
	public class PlayerController : IReceiver
	{
		// When moving using the keyboard, diagonal directions can be normalized by pre-computing this value (avoiding
		// an actual square root call at runtime).
		private const float SqrtTwo = 1.41421356237f;

		private PlayerCharacter player;
		private PlayerData playerData;
		private PlayerControls controls;
		private ControlSettings settings;

		private GroundController groundController;
		private AerialController aerialController;
		private PlatformController platformController;
		private LadderController ladderController;
		private WallController wallController;

		// Unlike other combat entities, player attacks are advanced during input processing (rather than the regular
		// update step).
		private Attack<PlayerCharacter> activeAttack;

		// It's possible for actions to have multiple binds. In cases where releasing a bind does something (e.g.
		// limiting a player's jump or releasing a hold), that action should only take place if the *same* bind was
		// released (rather than releasing a *different* button bound to the same action). In practice, then, that
		// means that while one bind is held in this scenario, other binds for that same action are ignored.
		private InputBind attackBindUsed;
		private InputBind jumpBindUsed;
		private InputBind grabBindUsed;
		private InputBind blockBindUsed;

		private InputBuffer attackBuffer;
		private InputBuffer grabBuffer;
		private InputBuffer ascendBuffer;

		public PlayerController(PlayerCharacter player, PlayerData playerData, PlayerControls controls,
			ControlSettings settings, AbstractController[] controllers)
		{
			this.player = player;
			this.playerData = playerData;
			this.controls = controls;
			this.settings = settings;

			aerialController = (AerialController)controllers[PlayerCharacter.ControllerIndexes.Air];
			groundController = (GroundController)controllers[PlayerCharacter.ControllerIndexes.Ground];
			platformController = (PlatformController)controllers[PlayerCharacter.ControllerIndexes.Platform];
			wallController = (WallController)controllers[PlayerCharacter.ControllerIndexes.Wall];
			ladderController = (LadderController)controllers[PlayerCharacter.ControllerIndexes.Ladder];

			// TODO: Make this class reloadable.
			// Create buffers.
			var accessor = Properties.Access();
			var grab = accessor.GetFloat("player.grab.buffer");
			var ascend = accessor.GetFloat("player.ascend.buffer");

			// Actual values for requiresHold on each buffer are set when control settings are applied.
			attackBuffer = new InputBuffer(false, controls.Attack);
			grabBuffer = new InputBuffer(grab, false, controls.Grab);
			ascendBuffer = new InputBuffer(ascend, false, controls.Jump);
			ascendBuffer.RequiredChords = controls.Ascend;

			settings.ApplyEvent += OnApply;

			MessageSystem.Subscribe(this, CoreMessageTypes.Input, (messageType, data, dt) =>
			{
				ProcessInput((FullInputData)data, dt);
			});
		}

		public FollowView FollowView { get; set; }
		public List<MessageHandle> MessageHandles { get; set; }

		public void Dispose()
		{
			MessageSystem.Unsubscribe(this);
		}

		private void OnApply(ControlSettings settings)
		{
			// TODO: Update binds on buffers.
			ascendBuffer.RequiresHold = !settings.UseToggleAscend;
			grabBuffer.RequiresHold = !settings.UseToggleGrab;
		}

		public void NullifyJumpBind()
		{
			jumpBindUsed = null;
		}

		private void ProcessInput(FullInputData data, float dt)
		{
			var flatDirection = ComputeFlatDirection(data);
			
			// Flat direction is on multiple controllers regardless of player state (so that if the player changes
			// state mid-step, movement still continues correctly).
			aerialController.FlatDirection = flatDirection;
			groundController.FlatDirection = flatDirection;
			platformController.FlatDirection = flatDirection;
			wallController.FlatDirection = flatDirection;
			
			// TODO: Make sure the call order is correct among all of these actions.
			activeAttack?.Update(dt);

			// The player can only interact while grounded. Interaction also takes priority over other actions on the
			// current frame.
			if ((player.State & PlayerStates.OnGround) > 0 && ProcessInteraction(data))
			{
				return;
			}

			ProcessLadder(data);

			// The jump button is used for multiple skills (including regular jumps, wall jump, and ascend). The order
			// in which these functions are called enforces the priority of those skills (ascend first, then wall jump,
			// then regular jumps).
			if (!(ProcessAscend(data, dt) || ProcessWallJump(data)))
			{
				ProcessJump(data, flatDirection);
			}

			//ProcessAttack(data, dt);
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
			
			return Utilities.Rotate(flatDirection, FollowView.Yaw);
		}

		private bool ProcessInteraction(FullInputData data)
		{
			return data.Query(controls.Interact, InputStates.PressedThisFrame) && player.TryInteract();
		}

		private void ProcessLadder(FullInputData data)
		{
			// Ladder climbing uses the same controls as running forward and back. Also note that directions remain the
			// same even if the camera is tilted down.
			bool up = data.Query(controls.RunForward, InputStates.Held);
			bool down = data.Query(controls.RunBack, InputStates.Held);

			ladderController.Direction = up ^ down ? (up ? 1 : -1) : 0;
		}

		private bool ProcessAscend(FullInputData data, float dt)
		{
			// There are two ascend-based actions the player can take: 1) starting an ascend (by holding the relevant
			// button and pressing jump), or 2) breaking out of an ongoing ascend (by pressing jump mid-ascend).
			if (!player.IsUnlocked(PlayerSkills.Ascend) || !ascendBuffer.Refresh(data, dt))
			{
				return false;
			}

			if ((player.State & PlayerStates.Ascending) == 0)
			{
				return player.TryAscend();
			}

			player.BreakAscend();

			return true;
		}

		/*
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
		*/

		private bool ProcessWallJump(FullInputData data)
		{
			if (player.IsWallJumpAvailable && data.Query(controls.Jump, InputStates.PressedThisFrame,
				out var bind))
			{
				player.WallJump();
				jumpBindUsed = bind;

				return true;
			}

			return false;
		}

		private void ProcessJump(FullInputData data, vec2 flatDirection)
		{
			// If this is true, it's assumed that the jump bind must have been populated. Note that this behavior (jump
			// limiting) applies to multiple kinds of jumps (including regular jumps, breaking ascends, wall jumps, and
			// maybe more).
			if ((player.State & PlayerStates.Jumping) > 0)
			{
				if (data.Query(jumpBindUsed, InputStates.ReleasedThisFrame) &&
				    player.ControllingBody.LinearVelocity.Y > playerData.JumpLimit)
				{
					player.LimitJump();
					jumpBindUsed = null;
				}

				return;
			}

			// This also accounts for jump being unlocked.
			if (player.JumpsRemaining == 0)
			{
				return;
			}

			if (data.Query(controls.Jump, InputStates.PressedThisFrame, out var bind))
			{
				player.Jump(flatDirection);
				jumpBindUsed = bind;
			}
		}

		private void ProcessAttack(FullInputData data, float dt)
		{
			var weapon = player.Weapon;

			if (attackBindUsed != null)
			{
				if (data.Query(attackBindUsed, InputStates.ReleasedThisFrame))
				{
					weapon.ReleasePrimary();
					attackBindUsed = null;
				}

				return;
			}

			// TODO: Process weapon cooldown as needed.
			if (attackBuffer.Refresh(data, dt, out var bind))
			{
				attackBindUsed = bind;
				activeAttack = weapon.TriggerPrimary();
			}
		}

		/*
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
		*/
	}
}
