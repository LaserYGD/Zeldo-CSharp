using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine;
using Engine.Core;
using Engine.Interfaces._3D;
using Engine.Physics;
using Engine.Sensors;
using Engine.Shapes._3D;
using Engine.Timing;
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
	// TODO: Add terminal velocity (maybe to all bodies).
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
		private LadderController ladderController;
		private WallController wallController;
		private ControlSettings settings;
		private DebugView debugView;

		private PlayerStates state;

		// Flags and timers
		private TimedFlag coyoteFlag;
		private TimedFlag coyoteWallFlag;
		private TimedFlag platformFlag;
		private SingleTimer wallStickTimer;

		//private IInteractive interactionTarget;
		//private IAscendable ascensionTarget;
		
		private bool[] skillsUnlocked;
		private bool[] upgradesUnlocked;
		private bool isJumpDecelerating;

		// The game is designed for double jumping, but is coded to accommodate any number of extra jumps.
		private int jumpsRemaining;

		public PlayerCharacter(ControlSettings settings) : base(EntityGroups.Player)
		{
			// TODO: Find a better way to retrieve settings.
			this.settings = settings;

			controls = new PlayerControls();
			playerData = new PlayerData();
			state = PlayerStates.Airborne;

			var coyoteTime = Properties.GetFloat("player.coyote.time");
			var coyoteWallTime = Properties.GetFloat("player.coyote.wall.time");
			var platformTime = Properties.GetFloat("player.platform.ignore.time");
			var wallStickTime = Properties.GetFloat("player.wall.stick.time");

			// Flags
			coyoteFlag = Components.Add(new TimedFlag(coyoteTime));
			coyoteFlag.OnExpiration = () => { jumpsRemaining--; };

			coyoteWallFlag = Components.Add(new TimedFlag(coyoteWallTime));
			coyoteWallFlag.OnExpiration = () => { wallController.Reset(); };

			platformFlag = new TimedFlag(platformTime);
			platformFlag.OnExpiration = () => { platformFlag.Tag = null; };

			wallStickTimer = new SingleTimer(time =>
			{
				UnstickFromWall();
			}, wallStickTime);

			int skillCount = Utilities.EnumCount<PlayerSkills>();
			int upgradeCount = Utilities.EnumCount<PlayerUpgrades>();

			skillsUnlocked = new bool[skillCount];
			upgradesUnlocked = new bool[upgradeCount];
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
		public FollowView FollowView
		{
			set => controller.FollowView = value;
		}

		// The player owns their own inventory.
		public Inventory Inventory { get; }

		// States are used by the controller class to more easily determine when to apply certain actions. Using a
		// single bitfield is more efficient than many booleans.
		public PlayerStates State => state;

		// These are used by the player controller.
		public int JumpsRemaining => jumpsRemaining;

		public bool IsWallJumpAvailable => IsUnlocked(PlayerSkills.WallJump) && ((state & PlayerStates.OnWall) > 0 ||
			coyoteWallFlag.Value);

		// This is used by the wall controller.
		public float CapsuleRadius => capsuleRadius;

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
			wallController = new WallController(this, wallStickTimer);

			// Ladder movement
			float ladderAcceleration = Properties.GetFloat("player.ladder.climb.acceleration");
			float ladderDeceleration = Properties.GetFloat("player.ladder.climb.deceleration");
			float ladderMaxSpeed = Properties.GetFloat("player.ladder.climb.max.speed");
			float ladderSeparation = Properties.GetFloat("player.ladder.separation");

			ladderController = new LadderController(this);
			ladderController.ClimbAcceleration = ladderAcceleration;
			ladderController.ClimbDeceleration = ladderDeceleration;
			ladderController.ClimbMaxSpeed = ladderMaxSpeed;
			ladderController.ClimbDistance = ladderSeparation + capsuleRadius;

			var controllers = new AbstractController[6];
			controllers[ControllerIndexes.Air] = aerialController;
			controllers[ControllerIndexes.Ground] = groundController;
			controllers[ControllerIndexes.Platform] = platformController;
			controllers[ControllerIndexes.Wall] = wallController;
			controllers[ControllerIndexes.Ladder] = ladderController;
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
			CreateSensor(scene, new Cylinder(FullHeight, capsuleRadius, false), SensorGroups.Player,
				SensorGroups.Interaction);
			
			controller = new PlayerController(this, playerData, controls, settings, CreateControllers());

			var canvas = scene.Canvas;
			healthDisplay = canvas.GetElement<PlayerHealthDisplay>();
			healthDisplay.Player = this;

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

			// While on a ladder, contacts should be negated with that ladder.
			if ((state & PlayerStates.OnLadder) > 0 && body == ladderController.Ladder.ControllingBody)
			{
				return false;
			}

			// While on a wall, contacts should be negated against that wall.
			if ((state & PlayerStates.OnWall) > 0)
			{
				var wallBody = wallController.Body;

				// This handles wall control against pseudo-static bodies.
				if (wallBody != null)
				{
					return body != wallBody;
				}

				var wall = wallController.Wall;

				// This handles wall control on static wall triangles.
				if (wall.IsSame(triangle))
				{
					return false;
				}

				// TODO: Consider finding a way to not recompute surface/normal data between this and Actor.
				var surfaceType = SurfaceTriangle.ComputeSurfaceType(triangle, WindingTypes.CounterClockwise);

				switch (surfaceType)
				{
					// While sliding on a wall, ground collisions are still handled using raycasting.
					case SurfaceTypes.Floor: return false;
					case SurfaceTypes.Ceiling: return true;
				}

				var n1 = wallController.Wall.Normal.swizzle.xz;
				var n2 = Utilities.ComputeNormal(triangle[0], triangle[1], triangle[2], WindingTypes.CounterClockwise,
					false).ToVec3().swizzle.xz;
				var angle = Utilities.Angle(n1, n2);

				// While sliding along a wall, the player only collides with walls at a sharp angle.
				return angle < PhysicsConstants.WallThreshold;
			}

			return base.ShouldGenerateContact(body, triangle);
		}

		public override bool OnContact(Entity entity, RigidBody body, vec3 p, vec3 normal, float penetration)
		{
			bool isAirborne = (state & PlayerStates.Airborne) > 0;

			// TODO: Does the player need to be facing a ladder to mount it? (raysB)
			// The player can attach to ladders by jumping towards them (from any side).
			if (isAirborne && entity is Ladder ladder && IsFacing(ladder))
			{
				Mount(ladder);

				return false;
			}

			return base.OnContact(entity, body, p, normal, penetration);
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

		protected override void OnLanding(vec3 p, vec3 n, RigidBody platform, SurfaceTriangle surface)
		{
			// TODO: Consider basing fall damage on both airtime and downward speed.
			if (controllingBody.LinearVelocity.Y < -playerData.FallDamageThreshold)
			{
				// TODO: If the player dies from fall damage, return early (and perform other actions).
				OnDamage(playerData.FallDamage);
			}

			base.OnLanding(p, n, platform, surface);

			// For upward-moving (or sloped) platforms, it's possible for the player to land while still in a jumping
			// state.
			state |= platform != null ? PlayerStates.OnPlatform : PlayerStates.OnGround;
			state &= ~(PlayerStates.Airborne | PlayerStates.Jumping | PlayerStates.OnWall);

			RefreshJumps();
			controller.NullifyJumpBind();
			wallStickTimer.Reset();
		}

		public override void BecomeAirborneFromLedge()
		{
			state |= PlayerStates.Airborne;
			state &= ~(PlayerStates.OnGround | PlayerStates.OnPlatform | PlayerStates.Running | PlayerStates.Sliding);

			coyoteFlag.Refresh();

			base.BecomeAirborneFromLedge();
		}

		public void BecomeAirborneFromWall()
		{
			state |= PlayerStates.Airborne;
			state &= ~PlayerStates.OnWall;

			activeController = aerialController;
			controllingBody.IsAffectedByGravity = true;
			controllingBody.IsManuallyControlled = false;

			if (manualBody != null)
			{
				manualBody = null;
				//controllingBody.LinearVelocity = ManualVelocity.ToJVector();
			}

			// TODO: Don't refresh the coyote wall flag when vaulting over the top of a wall.
			coyoteWallFlag.Refresh();
		}

		protected override void PreStep(float step)
		{
			// Flags are processed first (should result in more accurate timing).
			wallStickTimer.Update(step);
			platformFlag.Update(step);

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
			if ((state & PlayerStates.Airborne) > 0)
			{
				ProcessAerialWallContacts();
			}
		}

		// This function is only called if the player isn't currently on a wall.
		private void ProcessAerialWallContacts()
		{
			// Unlike the ground (which uses a single reference point), the player could hit multiple walls on the same
			// step. If that happens, only the closest wall is counted as a collision.
			var closestSquared = float.MaxValue;
			var closestPoint = vec3.Zero;
			var closestNormal = vec3.Zero;

			SurfaceTriangle closestWall = null;
			RigidBody closestBody = null;

			foreach (Arbiter arbiter in controllingBody.Arbiters)
			{
				var contacts = arbiter.ContactList;

				for (int i = contacts.Count - 1; i >= 0; i--)
				{
					var contact = contacts[i];
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
					else
					{
						// For the code below, b2 is meant to be the other body (not the player's controlling body).
						b2 = b1;
					}

					var surfaceType = SurfaceTriangle.ComputeSurfaceType(n);

					if (surfaceType != SurfaceTypes.Wall)
					{
						continue;
					}

					// TODO: Manage jumping into a corner (where on the step you gain wall control, you might already be embedded in an acute wall).
					// While processing aerial wall hits, all static (or pseudo-static) contacts are negated.
					contacts.RemoveAt(i);

					var tag = b2.Tag;
					var bodyP = controllingBody.Position.ToVec3();
					var oldBodyP = controllingBody.OldPosition.ToVec3();

					// TODO: Handle wall interaction on platforms as well.
					// This means that the player hit a wall on the static map mesh (rather than a platform).
					if (tag == null)
					{
						var triangle = contact.Triangle;
						var surface = new SurfaceTriangle(triangle, n, 0, surfaceType, true);

						// This determines the side of the triangle on which the relevant capsule point lies. If it's
						// opposite velocity, that means the point must have already passed through the plane.
						var offset = -surface.FlatNormal * capsuleRadius;
						var p1 = oldBodyP + offset;
						var p2 = bodyP + offset;

						// TODO: Figure out why the intersection function seems to be wrong.
						if (Utilities.Intersects(p1, p2, triangle, n, out var result))
						//if (PhysicsUtilities.Raycast(Scene.World, p1, p2, out var results))
						{
							// Only the closest wall is used to gain wall control.
							float squared = Utilities.DistanceSquared(result, bodyP);
							//float squared = Utilities.DistanceSquared(results.Position, p);

							if (squared < closestSquared)
							{
								closestSquared = squared;
								closestWall = surface;
								closestPoint = result;
								closestNormal = n;
								//closestPoint = results.Position;
							}
						}

						continue;
					}

					// TODO: Process static body wall movement as well (if needed).
					// This means the player hit the side of a platform (or other pseudo-static body, like a windmill
					// blade).
					if (b2.BodyType != RigidBodyTypes.PseudoStatic)
					{
						continue;
					}

					// Much of the code below is similar to the code above (used for wall control on the static map
					// mesh), but it's simpler just to partially duplicate it.
					var flatNormal = Utilities.Normalize(new vec3(n.x, 0, n.z));

					// TODO: Consider pulling back the starting point of the raycast by a small amount (above as well).
					var v = -flatNormal * capsuleRadius;
					var r1 = oldBodyP + v;
					var r2 = bodyP + v;

					if (PhysicsUtilities.Raycast(Scene.World, b2, r1, r2, out var results))
					{
						var point = results.Position;
						var squared = Utilities.DistanceSquared(point, bodyP);

						if (squared < closestSquared)
						{
							closestSquared = squared;
							closestBody = b2;
							closestPoint = point;
							closestNormal = n;
						}
					}
				}
			}

			// This means that at least one valid wall target (triangle or body) was successfully hit.
			if (closestWall != null || closestBody != null)
			{
				GainWallControl(closestWall, closestBody, closestPoint, closestNormal);
			}
		}

		private void GainWallControl(SurfaceTriangle surface, RigidBody body, vec3 p, vec3 n)
		{
			vec3 flatNormal;

			activeController = wallController;

			// The player can either can wall control on a static wall triangle or another body, but not both.
			if (surface != null)
			{
				// TODO: Retrieve the static world mesh in a simpler way.
				var mapBody = Scene.World.RigidBodies.First(b => b.IsStatic && b.Tag == null);

				wallController.Refresh(mapBody, surface);
				flatNormal = surface.FlatNormal;

				/*
				var v = controllingBody.LinearVelocity.ToVec3();
				v -= Utilities.Project(v, n);
				controllingBody.LinearVelocity = v.ToJVector();
				*/
			}
			else
			{
				// TODO: Consider optimizing to avoid the extra normalization.
				// This is technically wasteful (since it's also computed when processing wall contacts), but that's
				// probably fine.
				flatNormal = Utilities.Normalize(n.x, 0, n.z);
				wallController.Refresh(body, n, flatNormal);
			}

			var offset = flatNormal * capsuleRadius;
			var result = p + offset;

			controllingBody.Position = result.ToJVector();
			controllingBody.IsAffectedByGravity = false;

			// Wall control acts as manual control on pseudo-static bodies.
			if (body != null)
			{
				controllingBody.IsManuallyControlled = true;
				RefreshManual(p, n, body, offset);
			}

			state |= PlayerStates.OnWall;
			state &= ~(PlayerStates.Airborne | PlayerStates.Jumping | PlayerStates.Vaulting);

			controller.NullifyJumpBind();
			coyoteWallFlag.Reset();
			isJumpDecelerating = false;

			// Touching a wall restores double jump.
			if (IsUnlocked(PlayerSkills.DoubleJump))
			{
				jumpsRemaining = TargetJumps - 1;
			}
		}

		private void UnstickFromWall()
		{
			// TODO: Consider artifically boosting the player off the wall very slightly.
			state |= PlayerStates.Airborne;
			state &= ~PlayerStates.OnWall;
			
			activeController = aerialController;
			controllingBody.IsAffectedByGravity = true;

			// The wall controller's wall reference is kept until the coyote wall flag expires.
			coyoteWallFlag.Refresh();
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

		// TODO: This function assumes respawning from the death plane. Should be generalized.
		private void Respawn()
		{
			controllingBody.Position = new JVector(3, 4, 0);
			controllingBody.LinearVelocity = JVector.Zero;
		}

		public void Reset(vec3 p)
		{
			// Reset body.
			controllingBody.IsAffectedByGravity = true;
			controllingBody.IsManuallyControlled = false;
			controllingBody.Position = p.ToJVector();
			controllingBody.LinearVelocity = JVector.Zero;

			// Reset state.
			state = PlayerStates.Airborne;

			// Reset controllers.
			activeController = aerialController;
			ladderController.Ladder = null;
			platformController.Platform = null;
			wallController.Reset();
			Ground = null;
			manualBody = null;

			// Reset jumps.
			RefreshJumps();

			// Reset flags.
			coyoteFlag.Reset();
			coyoteWallFlag.Reset();
			platformFlag.Reset();
			wallStickTimer.Reset();
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

						// TODO: Nullify yaw speed as well.
						ManualVelocity = vec3.Zero;
						manualBody = null;

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

			// I'm pretty sure this logic is correct (resetting coyote flags whenever jumps are refreshed).
			coyoteFlag.Reset();
			coyoteWallFlag.Reset();
		}

		public void WallJump()
		{
			// The player can wall jump with some directional control (within an angle range of the wall's normal),
			// rather than strictly perpendicular to the wall.
			var wall = wallController.Wall;

			vec3 flatNormal;

			if (wall != null)
			{
				flatNormal = wall.FlatNormal;
			}
			else
			{
				var flat = wallController.Normal.swizzle.xz;
				flatNormal = Utilities.Normalize(flat.x, 0, flat.y);
			}

			var flatAngle = Utilities.Angle(flatNormal.swizzle.xz);
			var flatDirection = coyoteWallFlag.Value ? aerialController.FlatDirection : wallController.FlatDirection;

			float resultAngle;

			// If movement direction is neutral, the wall jump boosts directly away from the wall (at a 90-degree
			// angle).
			if (Utilities.LengthSquared(flatDirection) > 0)
			{
				var delta = Utilities.Delta(Utilities.Angle(flatDirection), flatAngle);
				var abs = Math.Abs(delta);
				var max = playerData.WallJumpMaxAngle;

				// The player can still aim wall jumps sideways when pressing towards the wall.
				if (abs > Constants.PiOverTwo)
				{
					delta = (Constants.Pi - abs) * Math.Sign(delta);
				}

				resultAngle = Utilities.Clamp(delta, -max, max);
				resultAngle += flatAngle;
			}
			else
			{
				resultAngle = flatAngle;
			}

			var s = Utilities.Direction(resultAngle) * playerData.WallJumpFlatSpeed;

			controllingBody.IsAffectedByGravity = true;
			controllingBody.IsManuallyControlled = false;
			controllingBody.LinearVelocity = new JVector(s.x, playerData.WallJumpYSpeed, s.y);

			// This is almost the same logic as unsticking from a wall, but it's simpler to just repeat it.
			state |= PlayerStates.Airborne | PlayerStates.Jumping;
			state &= ~PlayerStates.OnWall;

			wallController.Reset();
			activeController = aerialController;
			aerialController.IgnoreDeceleration = true;
			manualBody = null;

			wallStickTimer.Reset();
			coyoteWallFlag.Reset();
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

		// TODO: Generalize this function to mount a ladder from any situation (rather than just the air).
		public void Mount(Ladder ladder)
		{
			// TODO: Whip around as appropriate.
			Proximities proximity = ladder.ComputeProximity(position);

			RefreshJumps();

			// This small offset helps separate the capsule from the ladder.
			var o = ladder.Orientation;
			var v1 = o * vec3.UnitX * ladderController.ClimbDistance;
			var v2 = o * vec3.UnitY * (controllingBody.Position.Y - ladder.Position.y) / ladder.CosineTilt;
			var p = ladder.ControllingBody.Position + (v1 + v2).ToJVector();

			// Ladders are positioned by their bottom-center point.
			controllingBody.Position = p;
			controllingBody.LinearVelocity = JVector.Zero;
			controllingBody.IsAffectedByGravity = false;
			controllingBody.IsManuallyControlled = true;

			activeController = ladderController;
			ladderController.Ladder = ladder;

			// TODO: Compute the proper ladder offset.
			// TODO: Compute the proper ladder normal (probably stored based on the direction the ladder is facing).
			// TODO: Once computed, cancel out velocity that's not along the ladder (up or down).
			RefreshManual(p.ToVec3(), vec3.Zero, ladder.ControllingBody, vec3.Zero);

			// This covers mounting the ladder from any situation.
			state &= ~(PlayerStates.Airborne | PlayerStates.Jumping | PlayerStates.Running | PlayerStates.Vaulting);
			state |= PlayerStates.OnLadder;
		}

		public void Equip(Weapon<PlayerCharacter> weapon)
		{
			this.weapon = weapon;
		}

		public void Unlock(PlayerSkills skill)
		{
			Debug.Assert(!skillsUnlocked[(int)skill], $"Skill {skill} already unlocked.");

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
			Debug.Assert(!upgradesUnlocked[(int)upgrade], $"Upgrade {upgrade} already unlocked.");
		}

		public bool IsUnlocked(PlayerSkills skill)
		{
			return skillsUnlocked[(int)skill];
		}

		public bool IsUnlocked(PlayerUpgrades upgrade)
		{
			return upgradesUnlocked[(int)upgrade];
		}

		public override void Update(float dt)
		{
			base.Update(dt);

			var sensor = GetAttachment<Sensor>();
			var list = debugView.GetGroup("Player");
			list.Add("State: " + State);
			list.Add("Arbiters: " + controllingBody.Arbiters.Count);
			list.Add("Contacts: " + controllingBody.Arbiters.Sum(a => a.ContactList.Count));
			list.Add("Sensor: " + sensor.Contacts.Count);

			DrawAxes();

			/*
			// TODO: This logic should be re-examined (or maybe applied to all actors).
			if (position.x != oldPosition.x || position.z != oldPosition.z)
			{
				facing = Utilities.Normalize((position - oldPosition).swizzle.xz);
			}

			Scene.Primitives.DrawLine(Position, Position + new vec3(facing.x, 0, facing.y), Color.Cyan);
			*/
		}

		private void DrawAxes()
		{
			const int Length = 1;

			var p = position + new vec3(0, 1.1f, 0);
			var primitives = Scene.Primitives;
			primitives.DrawLine(p, p + vec3.UnitX * Length, Color.Red);
			primitives.DrawLine(p, p + vec3.UnitY * Length, Color.Green);
			primitives.DrawLine(p, p + vec3.UnitZ * Length, Color.Cyan);
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
			public const int Platform = 2;
			public const int Wall = 3;
			public const int Ladder = 4;
			public const int Swim = 5;
		}
	}
}
