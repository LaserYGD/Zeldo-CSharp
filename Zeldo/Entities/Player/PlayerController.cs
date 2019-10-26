using System.Collections.Generic;
using Engine;
using Engine.Input;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Timing;
using Engine.Utility;
using GlmSharp;
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
		private LadderController ladderController;
		private WallController wallController;

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

		public PlayerController(PlayerCharacter player, PlayerData playerData, PlayerControls controls,
			ControlSettings settings, AbstractController[] controllers)
		{
			this.player = player;
			this.playerData = playerData;
			this.controls = controls;
			this.settings = settings;

			aerialController = (AerialController)controllers[PlayerCharacter.ControllerIndexes.Air];
			groundController = (GroundController)controllers[PlayerCharacter.ControllerIndexes.Ground];
			wallController = (WallController)controllers[PlayerCharacter.ControllerIndexes.Wall];
			ladderController = (LadderController)controllers[PlayerCharacter.ControllerIndexes.Ladder];

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

		public void OnLanding()
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
			wallController.FlatDirection = flatDirection;

			ProcessLadder(data);

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

		private void ProcessJumping(FullInputData data, float dt)
		{
			// If this is true, it's assumed that the jump bind must have been populated. Note that this case also
			// handles breaking from ascend via a jump.
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
			/*
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
			*/
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

		private void ProcessInteraction(FullInputData data)
		{
			if (data.Query(controls.Interact, InputStates.PressedThisFrame))
			{
				player.TryInteract();
			}
		}
		*/
	}
}
