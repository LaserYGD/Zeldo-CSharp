using System;
using System.Linq;
using Engine.Physics;
using Engine.Utility;
using GlmSharp;
using Jitter;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Zeldo.Control;
using Zeldo.Physics;
using Zeldo.UI;

namespace Zeldo.Entities.Core
{
	// TODO: Do all actors need to process steps?
	public abstract class Actor : LivingEntity
	{
		private AbstractController activeController;

		private bool isOldPositionUnset;
		private float halfHeight;

		// TODO: Is this needed?
		protected bool isSurfaceControlOverridden;

		protected vec3 oldPosition;
		protected SurfaceController surfaceController;

		protected Actor(EntityGroups group) : base(group)
		{
			isOldPositionUnset = true;

			// TODO: Do all actors need a surface controller created, or only actors that move?
			surfaceController = new SurfaceController(this);
		}
		
		public float Height
		{
			get => halfHeight * 2;
			set => halfHeight = value / 2;
		}

		public override vec3 Position
		{
			get => base.Position;
			set
			{
				// Even while grounded, actors can be affected by the regular physics step (for example, running into
				// walls).
				// TODO: This world active check might not be needed (if using the aggregate method to resolve grounded wall collisions).
				if (OnSurface && !Scene.World.IsStepActive)
				{
					// This assumes that all actors will have a valid controlling body created.
					controllingBody.LinearVelocity = (value - controllingBody.Position.ToVec3()).ToJVector();
				}

				// Without this check, the first airborne raytrace (to handle collisions with surfaces) would be way
				// too large (unless the actor happens to spawn at the origin).
				if (isOldPositionUnset)
				{
					oldPosition = value;
					isOldPositionUnset = false;
				}
				else
				{
					oldPosition = position;
				}

				base.Position = value;
			}
		}

		public vec3 GroundPosition
		{
			get => position - new vec3(0, halfHeight, 0);
			set => Position = value + new vec3(0, halfHeight, 0);
		}

		// A separate velocity is used for controlled movement along surfaces (such as the ground). The controlling
		// body's velocity can't be easily reused because it'd constantly be affected by the physics engine.
		public vec3 SurfaceVelocity { get; set; }

		// This used to be "onGround", but was updated to account for any kind of surface (e.g. slimes that can crawl
		// on walls or ceilings).
		public bool OnSurface => surfaceController.Surface != null;

		protected RigidBody CreateKinematicBody(Scene scene, Shape shape)
		{
			var body = CreateBody(scene, shape, RigidBodyTypes.Kinematic);
			body.ShouldCollideWith = ShouldCollideWith;

			return body;
		}

		protected virtual bool ShouldCollideWith(RigidBody body, JVector[] triangle)
		{
			// Triangles are only sent into the callback for triangle mesh and terrain collisions. For now, collisions
			// are only ignored to accommodate surface movement (which also means that actors without a surface
			// controller created can return early).
			if (triangle == null || surfaceController == null)
			{
				return true;
			}

			var onSurface = OnSurface;
			var surfaceType = SurfaceTriangle.ComputeSurfaceType(triangle, WindingTypes.CounterClockwise);

			bool isPotentialLanding = !onSurface && surfaceType == SurfaceTypes.Floor;

			// TODO: This will cause glancing downward collisions to be ignored. Should they be?
			// Since actors use capsules, potential ground collisions are ignored from the air. Instead, raycasts are
			// used to determine when the exact bottom-center of the capsule crosses a triangle.
			if (isPotentialLanding)
			{
				return false;
			}

			// While on a surface, only collisions with surfaces of *different* types are processed. For example,
			// while grounded, only wall and ceiling collisions should occur (the surface controller handles movement
			// among surfaces of the same type).
			return onSurface && surfaceType != surfaceController.Surface.SurfaceType;
		}

		protected void Swap(AbstractController controller, bool shouldComputeImmediately = false)
		{
			activeController = controller;

			// Calling this function here ensures the actor will be positioned properly the moment the controller
			// changes (if needed).
			if (shouldComputeImmediately)
			{
				controller.Update(0);
			}
		}

		protected virtual void OnLanding(vec3 p, SurfaceTriangle surface)
		{
			// Note that by setting ground position *before* the onSurface flag, the body's velocity isn't wastefully
			// set twice (since it's forcibly set to zero below).
			surface.Project(p, out vec3 result);

			// Note: it's important to set the surface controller's surface before updating position (to ensure that
			// OnSurface returns true).
			surfaceController.Surface = surface;
			GroundPosition = result;

			// TODO: Account for speed differences when landing on slopes (since maximum flat speed will be a bit lower). Maybe quick deceleration?
			// The surface controller works off surface velocity, so the body's existing aerial velocity must be
			// transferred.
			var bodyVelocity = controllingBody.LinearVelocity;
			var v = SurfaceVelocity;
			v.x = bodyVelocity.X;
			v.z = bodyVelocity.Z;
			SurfaceVelocity = v;

			// TODO: Setting body position directly could cause rare collision misses on dynamic objects. Should be tested.
			controllingBody.Position = position.ToJVector();
			controllingBody.LinearVelocity = JVector.Zero;
			controllingBody.AffectedByGravity = false;

			Swap(surfaceController);
			OnSurfaceTransition(surface);
		}

		public virtual void OnSurfaceTransition(SurfaceTriangle surface)
		{
		}

		public virtual void BecomeAirborneFromLedge()
		{
		}

		public void PlayAnimation(string animation)
		{
		}

		public override void Update(float dt)
		{
			// TODO: Verify the ordering of method calls here.
			Components.Update(dt);
			selfUpdate = true;
			activeController?.Update(dt);

			// This handles actors landing from being airborne (using a downward raytrace from the bottom of the
			// capsule).
			if (!OnSurface && CheckGroundCollision(out var results))
			{
				// TODO: Retrieve material from the surface as well.
				OnLanding(results.Position, new SurfaceTriangle(results.Triangle, results.Normal, 0));
			}

			// Even while on a surface (i.e. using manual control), the physics engine can override movement when
			// certain collisions occur (e.g. hitting a wall). Using the override flag, then, helps keep the mesh and
			// rigid body in sync.
			if (!OnSurface || isSurfaceControlOverridden)
			{
				Position = controllingBody.Position.ToVec3();

				// TODO: If actor bodies are always fixed upright, is this needed?
				Orientation = controllingBody.Orientation.ToQuat();
				isSurfaceControlOverridden = false;
			}

			selfUpdate = false;
		}

		private bool CheckGroundCollision(out RaycastResults results)
		{
			results = null;

			var v = controllingBody.LinearVelocity;

			// TODO: If moving platforms are added, a relative velocity check will be needed.
			// TODO: Is this check needed anymore?
			// This prevents false collisions just after jumping.
			if (v.Y > 0)
			{
				return false;
			}

			// Since the player's body is a capsule, a ground landing is only considered valid if the bottom point
			// passes through a triangle (rather than any collision with the static world mesh).
			// TODO: Store easier references to static meshes on the scene.
			var world = Scene.World;
			var map = world.RigidBodies.First(b => b.Shape is TriangleMeshShape);

			vec3 halfVector = new vec3(0, halfHeight, 0);
			vec3 p1 = oldPosition - halfVector;
			vec3 p2 = Position - halfVector;

			results = PhysicsUtilities.Raycast(world, map, p1, p2);

			// Actors can only land on surfaces facing the actor (not back-facing ones).
			return results != null && Utilities.Dot(results.Normal, v.ToVec3()) < 0;
		}
	}
}
