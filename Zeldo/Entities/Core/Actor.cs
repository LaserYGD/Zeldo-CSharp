using System.Diagnostics;
using System.Linq;
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
	public abstract class Actor : LivingEntity
	{
		private bool isOldPositionUnset;

		protected AbstractController activeController;
		protected AerialController aerialController;
		protected GroundController groundController;

		// TODO: Set this appropriately for all entities.
		protected vec2 facing;

		protected Actor(EntityGroups group, bool canTraverseGround = true) : base(group)
		{
			isOldPositionUnset = true;
			aerialController = new AerialController(this);

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

		// Since it's common for multiple physics steps to occur per rendered frame (usually two), it's actually the
		// body's old position that needs to be stored, not the entity's.
		protected vec3 OldBodyPosition { get; private set; }

		public float Height { get; set; }

		public override vec3 Position
		{
			get => base.Position;
			set
			{
				if (isOldPositionUnset)
				{
					OldBodyPosition = value;
					isOldPositionUnset = false;
				}

				base.Position = value;
			}
		}

		// This is used by the ground controller.
		public SurfaceTriangle Ground { get; protected set; }

		protected virtual bool ShouldGenerateContact(RigidBody body, JVector[] triangle)
		{
			return false;

			// Triangles are only sent into the callback for triangle mesh and terrain collisions.
			if (triangle == null)
			{
				return true;
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
			Debug.Assert(!isOldPositionUnset, "Actors are expected to have their initial position set on spawn " +
				"(before physics updates begin).");

			OldBodyPosition = controllingBody.Position.ToVec3();
			activeController?.PreStep(step);
		}

		protected virtual void PostStep(float step)
		{
			if (groundController != null && Ground == null && CastGround(out var results))
			{
				// TODO: Retrieve material as well.
				OnLanding(results.Position, new SurfaceTriangle(results.Triangle, results.Normal, 0));
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
			results = null;

			var v = controllingBody.LinearVelocity;

			// TODO: If moving platforms are added, a relative velocity check will be needed.
			if (v.Y > 0)
			{
				return false;
			}

			// Since the player's body is a capsule, a ground landing is only processed if the bottom point passes
			// through a ground triangle.
			// TODO: Store better references to static meshes on the scene.
			var world = Scene.World;
			var map = world.RigidBodies.First(b => b.Shape is TriangleMeshShape);

			vec3 halfVector = new vec3(0, Height / 2, 0);
			vec3 p1 = OldBodyPosition - halfVector;
			vec3 p2 = controllingBody.Position.ToVec3() - halfVector;

			results = PhysicsUtilities.Raycast(world, map, p1, p2);

			if (results == null)
			{
				return false;
			}

			// Actors can only land on the ground (not on top of ceilings, which would be back-facing).
			return SurfaceTriangle.ComputeSurfaceType(results.Normal) == SurfaceTypes.Floor;
		}

		// This function is called via manual raycasting during the physics step.
		protected virtual void OnLanding(vec3 p, SurfaceTriangle surface)
		{
			// TODO: When jumping straight up and down on a sloped surface, XZ position can start to change very slowly. Should be fixed.
			Ground = surface;
			Ground.Project(p, out vec3 result);

			// TODO: Apply speed properly when landing on slopes (rather than setting Y to zero).
			var v = controllingBody.LinearVelocity;
			v.Y = 0;
			controllingBody.LinearVelocity = v;
			controllingBody.Position = result.ToJVector() + new JVector(0, Height / 2, 0);
			controllingBody.IsAffectedByGravity = false;

			activeController = groundController;

			OnGroundTransition(surface);
		}

		public virtual void OnGroundTransition(SurfaceTriangle ground)
		{
			Ground = ground;
		}

		// This is called when the actor runs or walks off an edge (without jumping).
		public virtual void BecomeAirborneFromGround()
		{
			Ground = null;
			controllingBody.IsAffectedByGravity = true;
			activeController = aerialController;
		}

		// TODO: Should a CreateMasterSensor function be created as well?
		protected RigidBody CreateMasterBody(Scene scene, Shape shape)
		{
			var body = CreateBody(scene, shape, RigidBodyTypes.Kinematic);
			body.ShouldGenerateContact = ShouldGenerateContact;
			//body.PreStep = PreStep;
			//body.PostStep = PostStep;
			body.IsRotationFixed = true;

			// TODO: Is this needed?
			body.AllowDeactivation = false;
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

		public override void Update(float dt)
		{
			// TODO: Verify the ordering of method calls here.
			Components.Update(dt);
			selfUpdate = true;
			Position = controllingBody.Position.ToVec3();
			selfUpdate = false;

			// Note that the base Update function is intentionally not called (it's easier to just duplicate a bit of
			// code here).
		}
	}
}
