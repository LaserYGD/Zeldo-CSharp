using System;
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
		private AbstractController activeController;

		private bool isOldPositionUnset;
		private float halfHeight;

		protected SurfaceTriangle ground;

		protected Actor(EntityGroups group) : base(group)
		{
			isOldPositionUnset = true;
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
				// Without this check, the first airborne raytrace (to handle collisions with surfaces) would be way
				// too large (unless the actor happens to spawn at the origin).
				if (isOldPositionUnset)
				{
					OldPosition = value;
					isOldPositionUnset = false;
				}
				else
				{
					OldPosition = position;
				}

				base.Position = value;
			}
		}

		// Since it's common for multiple physics steps to occur per rendered frame (usually two), it's actually the
		// body's old position that needs to be stored, not the entity's.
		protected vec3 OldBodyPosition { get; private set; }

		public vec3 GroundPosition
		{
			get => position - new vec3(0, halfHeight, 0);
			set => Position = value + new vec3(0, halfHeight, 0);
		}

		protected virtual bool ShouldCollideWith(RigidBody body, JVector[] triangle)
		{
			// Triangles are only sent into the callback for triangle mesh and terrain collisions.
			if (triangle == null)
			{
				return true;
			}

			var onGround = ground != null;
			var surfaceType = SurfaceTriangle.ComputeSurfaceType(triangle, WindingTypes.CounterClockwise);

			bool isPotentialLanding = !onGround && surfaceType == SurfaceTypes.Floor;

			// TODO: This will cause glancing downward collisions to be ignored. Should they be?
			// Since actors use capsules, potential ground collisions are ignored from the air. Instead, raycasts are
			// used to determine when the exact bottom-center of the capsule crosses a triangle.
			if (isPotentialLanding)
			{
				return false;
			}

			// While grounded, only wall and ceiling collisions should generate contacts.
			if (onGround)
			{
				return surfaceType != SurfaceTypes.Floor;
			}

			// This helps prevent phantom collisions while separating from a non-floor surface (or sliding around a
			// corner).
			var n = Utilities.ComputeNormal(triangle[0], triangle[1], triangle[2], WindingTypes.CounterClockwise,
				false);

			return JVector.Dot(controllingBody.LinearVelocity, n) < 0;
		}

		protected virtual void PreStep(float step)
		{
			OldBodyPosition = controllingBody.Position.ToVec3();
		}

		protected virtual void PostStep(float step)
		{
			if (CastGround(out var results))
			{
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

			vec3 halfVector = new vec3(0, halfHeight, 0);
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

		// This function is called via manual raycasting during the physics step.
		protected virtual void OnLanding(vec3 p, SurfaceTriangle surface)
		{
			// TODO: When jumping straight up and down on a sloped surface, XZ position can start to change very slowly. Should be fixed.
			surface.Project(p, out vec3 result);

			ground = surface;

			// TODO: Apply speed properly when landing on slopes (rather than setting Y to zero).
			var v = controllingBody.LinearVelocity;
			v.Y = 0;
			controllingBody.LinearVelocity = v;
			controllingBody.Position = result.ToJVector();
			controllingBody.IsAffectedByGravity = false;

			OnSurfaceTransition(surface);
		}

		protected virtual void OnSurfaceTransition(SurfaceTriangle surface)
		{
		}

		public virtual void BecomeAirborneFromLedge()
		{
		}

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

			/*
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
			*/

			// Note that the base Update function is intentionally not called (it's easier to just duplicate a bit of
			// code here).
		}
	}
}
