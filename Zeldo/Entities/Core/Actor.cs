using System;
using System.Diagnostics;
using System.Linq;
using Engine;
using Engine.Physics;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Zeldo.Control;
using Zeldo.Physics;

namespace Zeldo.Entities.Core
{
	// TODO: Do all actors need to process steps?
	// TODO: Consider storing a half-height vector (since it's used frequently).
	public abstract class Actor : LivingEntity
	{
		private float bodyYaw;

		protected AbstractController activeController;
		protected AerialController aerialController;
		protected GroundController groundController;
		protected PlatformController platformController;

		// TODO: Are both of these needed?
		protected float capsuleHeight;
		protected float capsuleRadius;

		// TODO: Set this appropriately for all entities.
		protected vec2 facing;

		// TODO: Use actor flags to optimize controller creation (and to return early from some functions).
		protected Actor(EntityGroups group, bool canTraverseGround = true) : base(group)
		{
			aerialController = new AerialController(this);
			platformController = new PlatformController(this);

			// Some actors can't run on the ground (like flying enemies), so it would be wasteful to create the ground
			// controller.
			if (canTraverseGround)
			{
				groundController = new GroundController(this);
			}

			// TODO: Consider allowing actors to spawn directly on the ground (via a raycast).
			// This assumes that all actors spawn in the air (likely just above the ground).
			activeController = aerialController;
			facing = vec2.UnitX;
		}

		public float FullHeight { get; set; }

		// TODO: Is this the best approach?
		// Actors can rotate around the Y axis, but that rotation is controlled manually. Storing yaw directly is more
		// efficient than recomputing it every step.
		public float BodyYaw
		{
			get => bodyYaw;
			set => bodyYaw = value;
		}

		// TODO: Should manual position be a vec3 instead?
		// This is used by the ground controller.
		public SurfaceTriangle Ground { get; protected set; }
		public JVector ManualPosition { get; set; }
		public vec3 ManualVelocity { get; set; }

		// Since actors are fixed vertically, only relative yaw (i.e. rotation around the Y axis) needs to be stored.
		public float ManualYaw { get; private set; }

		protected virtual bool ShouldGenerateContact(RigidBody body, JVector[] triangle)
		{
			// Triangles are only sent into the callback for triangle mesh and terrain collisions.
			if (triangle == null)
			{
				// While on a moving platform, contacts should not be generated with that platform.
				return platformController != null && body != platformController.Platform;
			}

			// Flying actors should collide with all static triangles (since they don't use surface control).
			if (groundController != null)
			{
				var surfaceType = SurfaceTriangle.ComputeSurfaceType(triangle, WindingTypes.CounterClockwise);

				// While grounded, only wall and ceiling collisions should generate contacts.
				if (Ground != null)
				{
					return surfaceType != SurfaceTypes.Floor;
				}

				bool isPotentialLanding = surfaceType == SurfaceTypes.Floor;

				// TODO: This will cause glancing downward collisions to be ignored. Should they be?
				// Since actors use capsules, potential ground collisions are ignored from the air. Instead, raycasts
				// are used to determine when the exact bottom-center of the capsule crosses a triangle.
				if (isPotentialLanding)
				{
					return false;
				}
			}

			// This helps prevent phantom collisions while separating from a non-floor surface (or sliding around a
			// corner).
			return !IsPhantomCollision(triangle);
		}

		protected bool IsPhantomCollision(JVector[] triangle)
		{
			// This helps prevent phantom collisions while separating from a non-floor surface (or sliding around a
			// corner).
			var n = Utilities.ComputeNormal(triangle[0], triangle[1], triangle[2], WindingTypes.CounterClockwise,
				false);

			return JVector.Dot(controllingBody.LinearVelocity, n) < 0;
		}

		protected virtual void PreStep(float step)
		{
			activeController?.PreStep(step);
		}

		protected virtual void MidStep(float step)
		{
			activeController?.MidStep(step);
		}

		protected virtual void PostStep(float step)
		{
			if (groundController != null && Ground == null && CastGround(out var results))
			{
				// TODO: Retrieve material as well.
				OnLanding(results.Position, null, new SurfaceTriangle(results.Triangle, results.Normal, 0));
			}
			// If the above condition is true, the ground controller will be active, and for the ground controller,
			// there's nothing to be done (in post-step) on the step the actor lands.
			else
			{
				activeController?.PostStep(step);
			}
		}

		private bool CastGround(out RaycastResults results)
		{
			var v = controllingBody.LinearVelocity;

			// TODO: If moving platforms are added, a relative velocity check will be needed.
			if (v.Y > 0)
			{
				results = null;

				return false;
			}

			// Since the player's body is a capsule, a ground landing is only processed if the bottom point passes
			// through a ground triangle.
			// TODO: Store better references to static meshes on the scene.
			var world = Scene.World;
			var map = world.RigidBodies.First(b => b.Shape is TriangleMeshShape);

			vec3 halfVector = new vec3(0, FullHeight / 2, 0);
			vec3 p1 = controllingBody.OldPosition.ToVec3() - halfVector;
			vec3 p2 = controllingBody.Position.ToVec3() - halfVector;

			if (!PhysicsUtilities.Raycast(world, map, p1, p2, out results))
			{
				return false;
			}

			// Actors can only land on the ground (not on top of ceilings, which would be back-facing).
			return SurfaceTriangle.ComputeSurfaceType(results.Normal) == SurfaceTypes.Floor;
		}

		public override bool OnContact(Entity entity, RigidBody body, vec3 p, vec3 normal, float penetration)
		{
			// TODO: Should actors be able to land on static bodies? (rather than only pseudo-static)
			// Actors can land on portions of any pseudo-static body that are flat enough to be considered a floor.
			if (body.BodyType != RigidBodyTypes.PseudoStatic || normal.y < 0)
			{
				return true;
			}

			// This is a safeguard against actors clipping through fast-moving, sloped platforms from the bottom.
			// Likely not perfect, but it should help (could also likely be alleviated through design).
			var contactY = p.y + normal.y * penetration;

			if (controllingBody.Position.Y - contactY < capsuleRadius)
			{
				return true;
			}

			// Similar to the ground mesh, actors only land on platforms when the bottom-center point touches.
			var halfHeight = new vec3(0, FullHeight / 2, 0);
			var p1 = controllingBody.OldPosition.ToVec3() - halfHeight;
			var p2 = controllingBody.Position.ToVec3() - halfHeight;

			// Previously, this function returned early if the relative velocity between the two bodies had the actor
			// moving up. In practice, however, upward platform movement (along with slopes and orientation changes)
			// can result in legitimate landings regardless of actor velocity. As such, some corrections need to be
			// applied before the raycast occurs.
			if (controllingBody.LinearVelocity.Y > 0)
			{
				var temp = p1;
				p1 = p2;
				p2 = temp;
			}

			// TODO: Does something need to be done if actor speed is zero?
			p1 += (body.Position - body.OldPosition).ToVec3();

			if (PhysicsUtilities.Raycast(Scene.World, body, p1, p2, out var results))
			{
				// TODO: Consider using dot products to determine surface type (angle is more expensive).
				// Verifying the result normal prevents false landings (often near the sides of platforms).
				float angle = Constants.PiOverTwo - Utilities.Angle(results.Normal, vec3.UnitY);

				if (angle > PhysicsConstants.WallThreshold)
				{
					OnLanding(results.Position, body, null);
				}
			}

			return false;
		}

		// This function is called both from landing on static entities (i.e. moving platforms) and via manual
		// raycasting during the physics step.
		protected virtual void OnLanding(vec3 p, RigidBody platform, SurfaceTriangle surface)
		{
			// TODO: Apply speed properly when landing on slopes (rather than setting Y to zero). Applies to both platforms and the ground.
			var v = controllingBody.LinearVelocity;
			v.Y = 0;
			controllingBody.LinearVelocity = v;

			// Platform and surface are mutually-exclusive here.
			if (platform != null)
			{
				// TODO: Transfer velocity better on platforms (so the player doesn't appear to instantly stop or speed up).
				controllingBody.Position = p.ToJVector() + new JVector(0, FullHeight / 2, 0);
				controllingBody.IsAffectedByGravity = false;
				controllingBody.IsManuallyControlled = true;

				activeController = platformController;
				platformController.Platform = platform;

				RecomputeManualOffsets(p, platform);

				return;
			}

			// TODO: When jumping straight up and down on a sloped surface, XZ position can start to change very slowly. Should be fixed.
			Ground = surface;
			Ground.Project(p, out vec3 result);

			controllingBody.Position = result.ToJVector() + new JVector(0, FullHeight / 2, 0);
			controllingBody.IsAffectedByGravity = false;

			activeController = groundController;

			OnGroundTransition(surface);
		}

		protected void RecomputeManualOffsets(vec3 p, RigidBody body)
		{
			var orientation = body.Orientation;

			ManualPosition = JVector.Transform(p.ToJVector() - body.Position, JMatrix.Inverse(orientation));
			ManualYaw = bodyYaw - orientation.ComputeYaw();
		}

		public virtual void OnGroundTransition(SurfaceTriangle ground)
		{
			Ground = ground;
		}

		// This is called when the actor runs or walks off an edge (without jumping). Applies to both the normal ground
		// and moving platform.s
		public virtual void BecomeAirborneFromLedge()
		{
			Ground = null;
			controllingBody.IsAffectedByGravity = true;
			controllingBody.IsManuallyControlled = false;
			activeController = aerialController;

			// TODO: Nullify yaw speed as well.
			ManualVelocity = vec3.Zero;

			if (platformController != null)
			{
				platformController.Platform = null;
			}
		}

		// TODO: Should a CreateMasterSensor function be created as well?
		protected RigidBody CreateMasterBody(Scene scene, Shape shape, bool isAffectedByGravity)
		{
			var flags = RigidBodyFlags.IsFixedVertical;

			if (isAffectedByGravity)
			{
				flags |= RigidBodyFlags.IsAffectedByGravity;
			}

			var body = CreateBody(scene, shape, RigidBodyTypes.Kinematic, flags);
			body.ShouldGenerateContact = ShouldGenerateContact;
			body.PreStep = PreStep;
			body.PostStep = PostStep;
			body.IsFixedVertical = true;

			// TODO: Is this needed?
			body.IsDeactivationAllowed = false;
			body.Damping = RigidBody.DampingType.None;

			// Restitution defaults to zero.
			var material = body.Material;
			material.KineticFriction = 0;
			material.StaticFriction = 0;

			return body;
		}

		public void PlayAnimation(string animation)
		{
		}
	}
}
