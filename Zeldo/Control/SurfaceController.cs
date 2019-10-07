using System;
using System.Linq;
using Engine;
using Engine.Physics;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Zeldo.Entities.Core;
using Zeldo.Physics;

namespace Zeldo.Control
{
	public class SurfaceController : AbstractController
	{
		private static readonly float EdgeForgiveness;

		static SurfaceController()
		{
			EdgeForgiveness = Properties.GetFloat("edge.forgiveness");
		}

		public SurfaceController(Actor parent) : base(parent)
		{
		}

		public SurfaceTriangle Surface { get; set; }

		public override void Update(float dt)
		{
			vec3 p = Parent.GroundPosition;
			vec3 v = Parent.SurfaceVelocity;

			if (v == vec3.Zero)
			{
				// Setting the same position again updates the body's velocity.
				Parent.GroundPosition = p;

				// This prevents very slow drift when standing still on sloped surfaces.
				return;
			}

			p += v * dt;

			// If the projection returns true, that means the actor is still within the current triangle.
			if (Surface.Project(p, out vec3 result))
			{
				Parent.GroundPosition = result;

				return;
			}

			// TODO: Store a reference to the physics map separately (rather than querying the world every frame).
			var world = Parent.Scene.World;
			var map = Parent.Scene.World.RigidBodies.First(b => b.Shape is TriangleMeshShape);
			var normal = Surface.Normal;

			// The raycast needs to be offset upward enough to catch steps.
			// TODO: Use properties for these raycast values.
			var results = PhysicsUtilities.Raycast(world, map, p + normal, -normal, 1.2f);

			// This means the actor moved to another triangle.
			if (results?.Triangle != null)
			{
				Surface = new SurfaceTriangle(results.Triangle, results.Normal, 0);
				Surface.Project(results.Position, out result);

				// TODO: Signal the actor of the surface transition (if needed).
				Parent.GroundPosition = result;
				Parent.OnSurfaceTransition(Surface);

				return;
			}

			// If the actor has moved past a surface triangle (without transitioning to another one), a very small
			// forgiveness distance is checked before signalling the actor to become airborne. This distance is
			// small enough to not be noticeable during gameplay, but protects against potential floating-point
			// errors near the seams of triangles.
			if (ComputeForgiveness(p, Surface) > EdgeForgiveness)
			{
				Parent.BecomeAirborneFromLedge();
			}
		}

		private float ComputeForgiveness(vec3 p, SurfaceTriangle surface)
		{
			// To compute the shortest distance to an edge of the triangle, points are rotated to a flat plane first
			// (using the surface normal).
			var q = Utilities.Orientation(surface.Normal, vec3.UnitY);
			var flatP = (q * p).swizzle.xz;
			var flatPoints = surface.Points.Select(v => (q * v).swizzle.xz).ToArray();
			var d = float.MaxValue;

			for (int i = 0; i < flatPoints.Length; i++)
			{
				var p1 = flatPoints[i];
				var p2 = flatPoints[(i + 1) % 3];

				d = Math.Min(d, Utilities.DistanceToLine(flatP, p1, p2));
			}

			return d;
		}
	}
}
