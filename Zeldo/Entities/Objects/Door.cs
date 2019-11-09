using System.Diagnostics;
using System.Linq;
using Engine;
using Engine.Physics;
using Engine.Shapes._3D;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Entities.Player;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Objects
{
	public class Door : Entity, IInteractive
	{
		private static readonly float AnimationSnap;
		private static readonly float RaycastLength;

		static Door()
		{
			AnimationSnap = Properties.GetFloat("door.animation.snap");
			RaycastLength = Properties.GetFloat("door.raycast.length");
		}

		private bool isLocked;

		public Door() : base(EntityGroups.Object)
		{
		}

		// TODO: Add door locking.
		public bool IsInteractionEnabled => true;
		public bool RequiresFacing => true;

		public override void Initialize(Scene scene, JToken data)
		{
			var bounds = CreateModel(scene, "Door.obj").Mesh.Bounds;
			var size = Properties.GetFloat("door.sensor.size");

			// TODO: Limit the body size to exclude the handle.
			CreateBody(scene, new BoxShape(bounds.ToJVector()), RigidBodyTypes.PseudoStatic);
			CreateSensor(scene, new Box(size, bounds.y, bounds.z, BoxFlags.IsFixedVertical), SensorGroups.Interaction);

			base.Initialize(scene, data);
		}

		public void OnInteract(Actor actor)
		{
			var v = orientation * vec3.UnitX;

			// This determins which side of the door is being opened.
			if (Utilities.Dot(v, actor.Position - position) < 0)
			{
				v *= -1;
			}

			// TODO: Retrieve static map body in a better way.
			var world = Scene.World;
			var body = world.RigidBodies.First(b => b.BodyType == RigidBodyTypes.Static &&
				b.Shape is TriangleMeshShape);

			if (!PhysicsUtilities.Raycast(world, body, position + v, -vec3.UnitY, RaycastLength, out var results))
			{
				Debug.Fail("Door raycast failed (this likely means that the raycast length needs to be extended).");
			}
			
			// TODO: To animate properly, probably need to mark the body manually-controlled and apply step.
			actor.GroundPosition = results.Position;
			actor.ControllingBody.LinearVelocity = JVector.Zero;

			var player = (PlayerCharacter)actor;
			player.Lock();

			// Attaching a null controller freezes the camera in place while the door opens.
			Scene.Camera.Attach(null);
		}
	}
}
