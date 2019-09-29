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

namespace Zeldo.Entities.Core
{
	// TODO: Do all actors need to process steps?
	public abstract class Actor : LivingEntity
	{
		private AbstractController activeController;

		private bool firstPositionSet;
		private float halfHeight;

		protected bool onGround;
		protected bool isSurfaceControlOverridden;

		// TODO: Once surface movement is fully transferred to this class (from Player), this variable could probably be public.
		protected vec3 oldPosition;

		protected Actor(EntityGroups group) : base(group)
		{
			firstPositionSet = true;
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
				if (onGround && !Scene.World.IsStepActive)
				{
					// This assumes that all actors will have a valid controlling body created.
					controllingBody.LinearVelocity = (value - controllingBody.Position.ToVec3()).ToJVector();
				}

				// Without this check, the first airborne raytrace (to handle collisions with surfaces) would be way
				// too large (unless the actor happens to spawn at the origin).
				if (firstPositionSet)
				{
					oldPosition = value;
					firstPositionSet = false;
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

		// A separate velocity is used for controlled movement along a surface (such as the ground). The controlling
		// body's velocity can't be easily reused because it'd constantly be affected by the physics engine.
		public vec3 SurfaceVelocity { get; set; }

		// This is used by external controllers.
		public bool OnGround => onGround;

		protected RigidBody CreateKinematicBody(Scene scene, Shape shape)
		{
			var body = CreateBody(scene, shape, RigidBodyTypes.Kinematic);
			body.ShouldIgnore = ShouldIgnore;

			return body;
		}

		// TODO: Consider swapping this custom callback to be positive (i.e. return true if the collision *should* occur).
		protected virtual bool ShouldIgnore(RigidBody other, JVector[] triangle)
		{
			bool isMesh = other.Shape is TriangleMeshShape;

			if (!isMesh)
			{
				// All non-mesh collisions should occur.
				return false;
			}

			var slope = Utilities.ComputeSlope(triangle);

			if (onGround)
			{
				// While already grounded, only wall collisions are registered.
				return Math.Abs(slope) < 0.5f;
			}

			// TODO: Use a property or constant instead.
			return slope < 0.5f;
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

		public void PlayAnimation(string animation)
		{
		}

		public override void Update(float dt)
		{
			// TODO: Verify the ordering of method calls here.
			Components.Update(dt);
			selfUpdate = true;
			activeController?.Update(dt);

			// It's assumed that all actors capable of going airborne have a controlling body set.
			if (!onGround)
			{
				if (CheckGroundCollision(out var results))
				{
					OnCollision(results.Position, results.Normal, results.Triangle, 0);
				}
				else
				{
					//Position = controllingBody.Position.ToVec3();
					//Orientation = controllingBody.Orientation.ToQuat();
				}
			}

			// Even while on a surface (i.e. using manual control), the physics engine can override movement when
			// certain collisions occur (e.g. hitting a wall). Using the override flag, then, helps keep the mesh and
			// rigid body in sync.
			if (!onGround || isSurfaceControlOverridden)
			{
				Position = controllingBody.Position.ToVec3();
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
