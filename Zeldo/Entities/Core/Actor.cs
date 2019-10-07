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
				/*
				// Even while grounded, actors can be affected by the regular physics step (for example, running into
				// walls).
				if (OnSurface)
				{
					// This assumes that all actors will have a valid controlling body created.
					//controllingBody.LinearVelocity = (value - controllingBody.Position.ToVec3()).ToJVector();
					controllingBody.Position = value.ToJVector();
				}
				*/

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
			body.IsRotationFixed = true;
			body.Damping = RigidBody.DampingType.None;

			// Restitution defaults to zero.
			var material = body.Material;
			material.KineticFriction = 0;
			material.StaticFriction = 0;

			return body;
		}

		protected virtual bool ShouldCollideWith(RigidBody body, JVector[] triangle)
		{
			// Triangles are only sent into the callback for triangle mesh and terrain collisions. For now, collisions
			// are only ignored to accommodate surface movement (which also means that actors without a surface
			// controller created can return early).
			//if (triangle == null || surfaceController == null)
			if (triangle == null)
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
			if (!(onSurface && surfaceType != surfaceController.Surface.SurfaceType))
			{
				return false;
			}

			// This helps prevent phantom collisions while separating from a surface (or sliding along a corner).
			var n = Utilities.ComputeNormal(triangle[0], triangle[1], triangle[2], WindingTypes.CounterClockwise,
				false);

			return JVector.Dot(controllingBody.LinearVelocity, n) < 0;
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
			// TODO: When jumping straight up and down on a sloped surface, XZ position can start to change very slowly. Should be fixed.
			// Note that by setting ground position *before* the onSurface flag, the body's velocity isn't wastefully
			// set twice (since it's forcibly set to zero below).
			surface.Project(p, out vec3 result);

			// Note: it's important to set the surface controller's surface before updating position (to ensure that
			// OnSurface returns true).
			surfaceController.Surface = surface;
			GroundPosition = result;

			// TODO: Account for speed differences when landing on slopes (since maximum flat speed will be a bit lower). Maybe quick deceleration?
			var v = controllingBody.LinearVelocity;
			v.Y = 0;
			controllingBody.LinearVelocity = v;
			controllingBody.Position = position.ToJVector();
			controllingBody.IsAffectedByGravity = false;

			Swap(surfaceController);
			OnSurfaceTransition(surface);
		}

		public virtual void OnSurfaceTransition(SurfaceTriangle surface)
		{
			surfaceController.Surface = surface;
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
			if (!OnSurface)// || isSurfaceControlOverridden)
			{
				Position = controllingBody.Position.ToVec3();
				//isSurfaceControlOverridden = false;
			}

			selfUpdate = false;

			// Note that the base Update function is intentionally not called (it's easier to just duplicate a bit of
			// code here).
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
