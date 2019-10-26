using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Core;
using Engine.Interfaces._3D;
using Engine.Physics;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Newtonsoft.Json.Linq;
using Zeldo.Control;
using Zeldo.Entities.Core;
using Zeldo.Entities.Weapons;
using Zeldo.Interfaces;
using Zeldo.Items;
using Zeldo.Physics;
using Zeldo.Settings;
using Zeldo.UI;
using Zeldo.UI.Hud;
using Zeldo.View;

namespace Zeldo.Entities.Player
{
	// TODO: Jumping is processed via jumpsRemaining, not the actual skill flags. Could consider optimizing this.
	public class PlayerCharacter : Actor
	{
		private const int AscendIndex = (int)PlayerSkills.Ascend;
		private const int JumpIndex = (int)PlayerSkills.Jump;
		private const int DoubleJumpIndex = (int)PlayerSkills.DoubleJump;

		// This value should never be changed normally, but could theoretically be modified after release to give
		// additional double jumps (for something like a randomizer).
		private const int TargetJumps = 2;
		
		private PlayerData playerData;
		private PlayerControls controls;
		private PlayerController controller;
		private PlayerHealthDisplay healthDisplay;
		private Weapon<PlayerCharacter> weapon;
		private WallController wallController;
		private DebugView debugView;

		private PlayerStates state;

		// Flags
		private TimedFlag coyoteFlag;
		private TimedFlag platformFlag;

		//private IInteractive interactionTarget;
		//private IAscendable ascensionTarget;
		
		private bool[] skillsUnlocked;
		private bool[] upgradesUnlocked;
		private bool isJumpDecelerating;

		// The game is designed for double jumping, but is coded to accommodate any number of extra jumps.
		private int jumpsRemaining;

		public PlayerCharacter(ControlSettings settings) : base(EntityGroups.Player)
		{
			controls = new PlayerControls();
			playerData = new PlayerData();
			state = PlayerStates.Airborne;

			var coyoteTime = Properties.GetFloat("player.coyote.time");
			var platformTime = Properties.GetFloat("player.platform.ignore.time");

			// Flags
			coyoteFlag = Components.Add(new TimedFlag(coyoteTime));
			coyoteFlag.OnExpiration = () => { jumpsRemaining--; };

			platformFlag = Components.Add(new TimedFlag(platformTime));
			platformFlag.OnExpiration = () => { platformFlag.Tag = null; };

			int skillCount = Utilities.EnumCount<PlayerSkills>();
			int upgradeCount = Utilities.EnumCount<PlayerUpgrades>();

			skillsUnlocked = new bool[skillCount];
			upgradesUnlocked = new bool[upgradeCount];
			controller = new PlayerController(this, playerData, controls, settings, CreateControllers());
		}

		public override vec3 Position
		{
			get => base.Position;
			set
			{
				// In this way, it's impossible for the player to ever actually touch the kill plane (since they're
				// immediately respawned instead).
				if (value.y <= playerData.KillPlane)
				{
					Respawn();

					return;
				}

				base.Position = value;
			}
		}

		// TODO: Could this be cleaned up?
		// This is required to move in the direction of camera aim (passed through to the controller class).
		public FollowController FollowController
		{
			set => controller.FollowController = value;
		}

		// The player owns their own inventory.
		public Inventory Inventory { get; }
		public SurfaceTriangle Wall { get; private set; }

		// States are used by the controller class to more easily determine when to apply certain actions. Using a
		// single bitfield is more efficient than many booleans.
		public PlayerStates State => state;

		// This is used by the player controller.
		public int JumpsRemaining => jumpsRemaining;

		private AbstractController[] CreateControllers()
		{
			// Air movement
			float airAcceleration = Properties.GetFloat("player.air.acceleration");
			float airDeceleration = Properties.GetFloat("player.air.deceleration");
			float airMaxSpeed = Properties.GetFloat("player.air.max.speed");

			aerialController.Initialize(airAcceleration, airDeceleration, airMaxSpeed);

			// Ground movement
			float groundAcceleration = Properties.GetFloat("player.ground.acceleration");
			float groundDeceleration = Properties.GetFloat("player.ground.deceleration");
			float groundMaxSpeed = Properties.GetFloat("player.ground.max.speed");

			groundController.Initialize(groundAcceleration, groundDeceleration, groundMaxSpeed);

			// Wall movement
			wallController = new WallController(this);

			// Ladder movement
			float ladderAcceleration = Properties.GetFloat("player.ladder.climb.acceleration");
			float ladderDeceleration = Properties.GetFloat("player.ladder.climb.deceleration");
			float ladderMaxSpeed = Properties.GetFloat("player.ladder.climb.max.speed");
			float ladderDistance = Properties.GetFloat("player.ladder.distance");

			// Create controllers.
			/*
			ladderController = new LadderController(this);
			ladderController.ClimbAcceleration = ladderAcceleration;
			ladderController.ClimbDeceleration = ladderDeceleration;
			ladderController.ClimbMaxSpeed = ladderMaxSpeed;

			// TODO: Use half ladder depth (rather than hardcoded).
			ladderController.ClimbDistance = ladderDistance + capsuleRadius + 0.05f;
			*/

			var controllers = new AbstractController[5];
			controllers[ControllerIndexes.Air] = aerialController;
			controllers[ControllerIndexes.Ground] = groundController;
			controllers[ControllerIndexes.Wall] = wallController;
			controllers[ControllerIndexes.Ladder] = null; //ladderController;
			controllers[ControllerIndexes.Swim] = null; //swimController;

			return controllers;
		}

		public override void Initialize(Scene scene, JToken data)
		{
			// This is the height of the cylinder (excluding the two rounded caps).
			capsuleHeight = Properties.GetFloat("player.capsule.height");
			capsuleRadius = Properties.GetFloat("player.capsule.radius");

			// TODO: This shouldn't need to be manually computed per actor. Should probably use a function.
			FullHeight = capsuleHeight + capsuleRadius * 2;

			CreateModel(scene, "Capsule.obj");
			CreateMasterBody(scene, new CapsuleShape(capsuleHeight, capsuleRadius), true);
				
			var canvas = scene.Canvas;
			healthDisplay = canvas.GetElement<PlayerHealthDisplay>();
			debugView = canvas.GetElement<DebugView>();

			base.Initialize(scene, data);
		}

		protected override bool ShouldGenerateContact(RigidBody body, JVector[] triangle)
		{
			// While active, the platform flag tracks the most recent platform (after jumping).
			if (platformFlag.Value && body == (RigidBody)platformFlag.Tag)
			{
				return false;
			}

			return base.ShouldGenerateContact(body, triangle);

			/*
			// While grounded or airborne, the same base logic applies.
			if (Wall == null)
			{
				return base.ShouldGenerateContact(body, triangle);
			}

			var surfaceType = SurfaceTriangle.ComputeSurfaceType(triangle, WindingTypes.CounterClockwise);

			switch (surfaceType)
			{
				case SurfaceTypes.Ceiling: return true;
				case SurfaceTypes.Floor: return false;
			}
			*/
		}

		public override bool OnContact(Entity entity, vec3 p, vec3 normal, float penetration)
		{
			bool isAirborne = (state & (PlayerStates.Airborne | PlayerStates.Jumping)) > 0;

			// The player can attach to ladders by jumping towards them (from any side).
			if (isAirborne && entity is Ladder ladder && IsFacing(ladder))
			{
				Mount(ladder);

				return false;
			}

			return base.OnContact(entity, p, normal, penetration);
		}

		/*
		private void OnGroundedWallCollision(vec3 p, vec3 normal)
		{
			// TODO: Should probably override ShouldCollideWith instead (to negate the step collision entirely).
			bool isStep = p.y - GroundPosition.y <= playerData.StepThreshold;

			if (isStep)
			{
				return;
			}

			// If the player hits a wall with a velocity close to perpendicular, the player stops and presses
			// against the wall. Once in that state, the player will remain pressed until velocity moves outside
			// a small angle threshold.
			float angleN = Utilities.Angle(normal.swizzle.xz);
			float angleV = Utilities.Angle(controllingBody.LinearVelocity.ToVec3().swizzle.xz);

			// TODO: Verify that the wall isn't a vault target.
			// TODO: Verify that the collision point is wide enough to press (i.e. not a glancing hit).
			if (Utilities.Delta(angleN, angleV) <= playerData.WallPressThreshold)
			{
				PressAgainstWall();
			}
		}
		*/

		protected override void OnLanding(vec3 p, RigidBody platform, SurfaceTriangle surface)
		{
			base.OnLanding(p, platform, surface);

			// For upward-moving (or sloped) platforms, it's possible for the player to land while still in a jumping
			// state.
			state |= (platform != null ? PlayerStates.OnPlatform : PlayerStates.OnGround);
			state &= ~(PlayerStates.Airborne | PlayerStates.Jumping);

			RefreshJumps();
			controller.OnLanding();
		}

		private void OnWallControlGained()
		{
			activeController = wallController;

			var v = controllingBody.LinearVelocity;
			controllingBody.IsAffectedByGravity = false;
			controllingBody.LinearVelocity = v;

			state |= PlayerStates.OnWall;
			state &= ~(PlayerStates.Airborne | PlayerStates.Jumping | PlayerStates.Vaulting);
		}

		private void OnWallLanding()
		{
		}

		public override void BecomeAirborneFromLedge()
		{
			state |= PlayerStates.Airborne;
			state &= ~(PlayerStates.OnGround | PlayerStates.OnPlatform | PlayerStates.Running | PlayerStates.Sliding);

			coyoteFlag.Refresh();

			base.BecomeAirborneFromLedge();
		}

		protected override void PreStep(float step)
		{
			base.PreStep(step);

			var v = controllingBody.LinearVelocity;

			// The player is considered running if grounded and moving (but not sliding).
			if (Ground != null && v.LengthSquared() > 0 && (state & PlayerStates.Sliding) == 0)
			{
				state |= PlayerStates.Running;
			}
			else
			{
				state &= ~PlayerStates.Running;
			}

			// Process jumps.
			if (isJumpDecelerating && DecelerateJump(step))
			{
				isJumpDecelerating = false;
			}
			// If the player's velocity moves below the limit (even while holding the jump button), state is
			// transitioned to pure airborne.
			else if ((state & PlayerStates.Jumping) > 0 && v.Y <= playerData.JumpLimit)
			{
				state &= ~PlayerStates.Jumping;
			}
		}

		private bool DecelerateJump(float step)
		{
			// In pre-step (where this function is called), gravity hasn't been applied by the physics engine yet. As
			// such, gravity needs to be manually added to the limit here such that, if the limit was reached during
			// jump deceleration, the resulting final velocity will be accurate.
			var limit = playerData.JumpLimit + PhysicsConstants.Gravity * step;
			var v = controllingBody.LinearVelocity;

			v.Y -= playerData.JumpDeceleration * step;

			bool result = false;

			if (v.Y <= limit)
			{
				v.Y = limit;

				// Returning true means that the jump has finished decelerating.
				result = true;
			}

			controllingBody.LinearVelocity = v;

			return result;
		}

		private void PressAgainstWall()
		{
		}

		protected override void MidStep(float step)
		{
			if ((state & PlayerStates.Airborne) > 0 && ProcessAerialWallContacts())
			{
				OnWallControlGained();
			}
		}

		// This function is only called if the player isn't currently on a wall.
		private bool ProcessAerialWallContacts()
		{
			var velocity = controllingBody.LinearVelocity.ToVec3();

			foreach (Arbiter arbiter in controllingBody.Arbiters)
			{
				var contacts = arbiter.ContactList;

				for (int i = contacts.Count - 1; i >= 0; i--)
				{
					var contact = contacts[i];

					// TODO: The player should be able to wall jump off the side of moving platforms as well.
					if (contact.Triangle == null)
					{
						continue;
					}

					var b1 = contact.Body1;
					var b2 = contact.Body2;

					if (!(b1.IsStatic || b2.IsStatic))
					{
						continue;
					}

					var n = contact.Normal.ToVec3();

					if (controllingBody == b1)
					{
						n *= -1;
					}

					// TODO: Could be optimized a bit by not constructing the full surface if it's a non-wall.
					// TODO: Could also be optimized by computing the flat normal only as needed.
					var surface = new SurfaceTriangle(contact.Triangle, n, 0);

					if (surface.SurfaceType != SurfaceTypes.Wall)
					{
						continue;
					}

					// This determines the side of the triangle on which the relevant capsule point lies. If it's
					// opposite velocity, that means the point must have already passed through the plane.
					var p = controllingBody.Position.ToVec3() - surface.FlatNormal * capsuleRadius;
					var v = p - surface.Points[0];
					var d = Utilities.Dot(v.swizzle.xz, velocity.swizzle.xz);

					// Being equal to zero should be impossible here, but it's safer to check anyway.
					if (d >= 0)
					{
						continue;
					}

					if (surface.Project(p, out var result))
					{
						controllingBody.Position = result.ToJVector();
						controllingBody.LinearVelocity = JVector.Zero;

						Wall = surface;

						// Returning true means the player did successfully collide with a wall.
						return true;
					}
				}
			}

			return false;
		}

		protected override void PostStep(float step)
		{
			base.PostStep(step);

			// This catches kill plane hits during a physics step.
			if (controllingBody.Position.Y - FullHeight / 2 <= playerData.KillPlane)
			{
				Respawn();
			}
			// This catches Y velocity changes from gravity (which haven't been applied yet in pre-step).
			else if ((state & PlayerStates.Jumping) > 0 && controllingBody.LinearVelocity.Y <= playerData.JumpLimit)
			{
				isJumpDecelerating = false;
				state &= ~PlayerStates.Jumping;
			}
		}

		private void Respawn()
		{
		}

		// TODO: Re-enable sliding later (if sliding is actually kept in the game).
		/*
		protected override void OnGroundTransition(SurfaceTriangle surface)
		{
			// Moving to a surface flat enough for normal running.
			if (surface.Slope < playerData.SlideThreshold)
			{
				State = PlayerStates.Running;

				return;
			}

			// Moving to a surface steep enough to cause sliding.
			State = PlayerStates.Sliding;
		}
		*/

		public void Jump()
		{
			// Jumps are decremented regardless of single vs. double jump.
			jumpsRemaining--;

			if (!skillsUnlocked[DoubleJumpIndex] || jumpsRemaining == TargetJumps - 1)
			{
				SingleJump();
			}
			else
			{
				DoubleJump();
			}

			state |= PlayerStates.Jumping | PlayerStates.Airborne;
		}

		// TODO: Consider adding a Mario-style higher jump after two in a row.
		private void SingleJump()
		{
			var v = controllingBody.LinearVelocity;
			var isGrounded = (state & (PlayerStates.OnGround | PlayerStates.OnPlatform)) > 0;

			if (isGrounded || coyoteFlag.Value)
			{
				v.Y = playerData.JumpSpeed;

				if (isGrounded)
				{
					var platform = platformController.Platform;

					// TODO: If the platform is moving up (faster than a threshold), apply an additional upward boost on jump.
					if (platform != null)
					{
						// TODO: Consider only preserving XZ momentum if speed is fast enough (to prevent the equivalent of an ultra-slow bash glide).
						// The player can maintain momentum when jumping off moving platforms.
						aerialController.IgnoreDeceleration = true;
						controllingBody.IsManuallyControlled = false;

						// TODO: Should the boost be linearly based on upward speed instead.
						// The player receives a more powerful jump off upward-moving platforms, but only if the
						// platform is moving fast enough.
						if (platform.LinearVelocity.Y >= playerData.PlatformJumpThreshold)
						{
							v.Y = playerData.PlatformJumpSpeed;
						}

						// Without this flag, when on a fast-moving, sloped platform, it's possible for the player's
						// jump to immediately hit the platform again (making it difficult to escape).
						platformFlag.Refresh(platform);
					}

					Ground = null;
					platformController.Platform = null;
					state &= ~(PlayerStates.OnGround | PlayerStates.OnPlatform | PlayerStates.Running |
						PlayerStates.Sliding);
				}
			}
			// TODO: Finish ladder jumping.
			else if ((state & PlayerStates.OnLadder) > 0)
			{
			}
			// TODO: Finish ascend jumping.
			else if ((state & PlayerStates.Ascending) > 0)
			{
			}

			controllingBody.LinearVelocity = v;
			controllingBody.IsAffectedByGravity = true;
			activeController = aerialController;

			coyoteFlag.Reset();
		}

		private void DoubleJump()
		{
			var v = controllingBody.LinearVelocity;
			v.Y = playerData.DoubleJumpSpeed;
			controllingBody.LinearVelocity = v;
		}

		// This function is only called when limiting actually has to occur (i.e. the player's velocity is checked in
		// advance).
		public void LimitJump()
		{
			isJumpDecelerating = true;
			state &= ~PlayerStates.Jumping;
		}

		private void RefreshJumps()
		{
			jumpsRemaining = skillsUnlocked[DoubleJumpIndex] ? TargetJumps : 1;
			isJumpDecelerating = false;

			// I'm pretty sure this logic is correct (whenever jumps are refreshed, the coyote flag should be reset as
			// well).
			coyoteFlag.Reset();
		}

		public bool TryAscend()
		{
			// The player can ascend while climbing ladders normally.
			if ((state & PlayerStates.OnLadder) > 0)
			{
				// TODO: Trigger the ascension.
				//Ascend(ladderController.Ladder);
				state &= ~PlayerStates.OnLadder;

				return true;
			}

			/*
			foreach (var contact in sensor.Contacts)
			{
				if (contact.Owner is IAscendable target)
				{
					Ascend(target);

					return true;
				}
			}
			*/

			return false;
		}

		// TODO: Finish the ascend skill.
		private void Ascend(IAscendable target)
		{
			state |= PlayerStates.Ascending;

			/*
			ascensionTarget = target;
			State = PlayerStates.Ascending;
			skillsEnabled[AscendIndex] = false;
			*/

			RefreshJumps();
		}

		public void BreakAscend()
		{
		}

		public bool TryGrab()
		{
			/*
			// The player must be facing the target in order to grab it.
			if (!sensor.GetContact(out IGrabbable target) || !IsFacing(target))
			{
				return false;
			}
			*/

			state |= PlayerStates.Grabbing;
			state &= ~PlayerStates.Running;

			return true;
		}

		public void ReleaseGrab()
		{
		}

		public void TryInteract()
		{
			// TODO: Handle multiple interactive targets (likely with a swap, similar to Dark Souls).
			/*
			if (!sensor.GetContact(out IInteractive target) || !target.IsInteractionEnabled)
			{
				return;
			}

			if (!target.RequiresFacing || IsFacing(target))
			{
				target.OnInteract(this);
			}
			*/

			state |= PlayerStates.Interacting;
			state &= ~PlayerStates.Running;
		}

		private bool IsFacing(IPositionable3D target)
		{
			// TODO: Should probably narrow the spread that counts as facing a target.
			return Utilities.Dot(target.Position.swizzle.xz - position.swizzle.xz, facing) > 0;
		}

		public void Equip(Weapon<PlayerCharacter> weapon)
		{
			this.weapon = weapon;
		}

		public bool IsUnlocked(PlayerSkills skill)
		{
			return skillsUnlocked[(int)skill];
		}

		/*
		public void Block()
		{
			IsBlocking = true;
		}

		public void Unblock()
		{
			IsBlocking = false;
		}

		public void Parry()
		{
		}
		*/

		public void Mount(Ladder ladder)
		{
			// TODO: Whip around as appropriate.
			Proximities proximity = ladder.ComputeProximity(position);

			/*
			ladderController.OnMount(ladder, this);
			Swap(ladderController);
			*/

			RefreshJumps();

			controllingBody.IsAffectedByGravity = false;

			// This covers mounting the ladder from any situation.
			state &= ~(PlayerStates.Airborne | PlayerStates.Jumping | PlayerStates.Running | PlayerStates.Vaulting);
			state |= PlayerStates.OnLadder;
		}

		public void Unlock(PlayerSkills skill)
		{
			skillsUnlocked[(int)skill] = true;

			switch (skill)
			{
				case PlayerSkills.Jump:
				case PlayerSkills.DoubleJump:
					// This covers any non-airborne situation (all of which refresh jumps).
					if ((state & PlayerStates.Airborne) == 0)
					{
						jumpsRemaining = skill == PlayerSkills.Jump ? 1 : TargetJumps;
					}

					break;
			}
		}

		public void Unlock(PlayerUpgrades upgrade)
		{
		}

		public override void Update(float dt)
		{
			// TODO: Limit jumps and update ascends (should these happen during the physics step instead?)
			/*
			else switch (state)
			{
				case PlayerStates.Jumping when controllingBody.LinearVelocity.Y <= playerData.JumpLimit:
					State = PlayerStates.Airborne;
					break;


				case PlayerStates.Ascending:
					UpdateAscend(dt);
					break;
			}
			*/

			base.Update(dt);

			var v = controllingBody.LinearVelocity;
			var angular = controllingBody.AngularVelocity;
			var list = debugView.GetGroup("Player");
			list.Add("State: " + State);
			list.Add("Arbiters: " + controllingBody.Arbiters.Count);
			list.Add("Contacts: " + controllingBody.Arbiters.Sum(a => a.ContactList.Count));
			list.Add($"Linear: {v.X:F3} {v.Y:F3} {v.Z:F3}");
			list.Add($"Angular: {angular.X:F3} {angular.Y:F3} {angular.Z:F3}");

			/*
			var p = controllingBody.Position.ToVec3();
			var v = controllingBody.LinearVelocity.ToVec3();
			var entries = new []
			{
				$"P Position: {Position.x:N2}, {Position.y:N2}, {Position.z:N2}",
				$"B position: {p.x:N2}, {p.y:N2}, {p.z:N2}",
				$"Old position: {oldPosition.x:N2}, {oldPosition.y:N2}, {oldPosition.z:N2}",
				$"Surface velocity: {SurfaceVelocity.x:N2}, {SurfaceVelocity.y:N2}, {SurfaceVelocity.z:N2}",
				$"Body velocity: {v.x:N2}, {v.y:N2}, {v.z:N2}",
				$"Flat direction: {controller.FlatDirection}",
				$"Arbiters: {controllingBody.Arbiters.Count}",
				$"Contacts: {controllingBody.Arbiters.Sum(a => a.ContactList.Count)}",
				$"On surface: {OnSurface}",
				$"Jumps remaining: {jumpsRemaining}"
			};

			debugView.GetGroup("Player").AddRange(entries);

			base.Update(dt);

			// TODO: This logic should be re-examined (or maybe applied to all actors).
			if (position.x != oldPosition.x || position.z != oldPosition.z)
			{
				facing = Utilities.Normalize((position - oldPosition).swizzle.xz);
			}

			Scene.DebugPrimitives.DrawLine(Position, Position + new vec3(facing.x, 0, facing.y), Color.Cyan);
			*/
		}

		/*
		private void UpdateAscend(float dt)
		{
			// The body's linear velocity is reused for ascension.
			var v = controllingBody.LinearVelocity;
			v.Y += playerData.AscendAcceleration * dt;

			if (v.Y > playerData.AscendTargetSpeed)
			{
				v.Y = playerData.AscendTargetSpeed;
			}

			controllingBody.LinearVelocity = v;
		}
		*/

		public static class ControllerIndexes
		{
			public const int Air = 0;
			public const int Ground = 1;
			public const int Wall = 2;
			public const int Ladder = 3;
			public const int Swim = 4;
		}
	}
}
