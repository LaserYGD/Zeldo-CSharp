using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine;
using Engine.Core;
using Engine.Core._3D;
using Engine.Graphics._3D;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.Physics;
using Engine.Sensors;
using Engine.Shapes._3D;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Newtonsoft.Json.Linq;

namespace Zeldo.Entities.Core
{
	// TODO: Consider adding a flag to disable updates (for things like non-moving ladders).
	public abstract class Entity : ITransformable3D, IDynamic, IDisposable
	{
		private quat orientation;
		private List<EntityAttachment> attachments;
		private List<EntityHandle> handles;

		// Using these variables helps ensure that static (and pseudo-static) physics bodies don't have their
		// transforms set twice.
		protected bool isSpawnPositionSet;
		protected bool isSpawnOrientationSet;

		protected vec3 position;
		protected RigidBody controllingBody;

		// The entity's transform properties (Position and Orientation) also update attachments. Since the controlling
        // physics body is itself an attachment, this would cause bodies to set their own transforms again (which is
        // wasteful). Using this variable avoids that.
	    protected bool selfUpdate;

		protected Entity(EntityGroups group)
		{
			Group = group;
			orientation = quat.Identity;
			attachments = new List<EntityAttachment>();
			Components = new ComponentCollection();

			// -1 means that the entity had no explicit ID given when spawned from a fragment file.
			Id = -1;
		}
		
		protected ComponentCollection Components { get; }

		public EntityGroups Group { get; }

		// ID isn't set on all entities (only those meant to be retrieved via handles).
		public int Id { get; private set; }

		public Scene Scene { get; protected set; }
		public RigidBody ControllingBody => controllingBody;

		public virtual vec3 Position
		{
			get => position;
			set
			{
				Debug.Assert(!(controllingBody != null && controllingBody.BodyType == RigidBodyTypes.Static &&
					isSpawnPositionSet), "For static entities (i.e. entities with a static controlling body), " +
					"position can only be set on spawn.");

				position = value;
				attachments.ForEach(a => a.Target.Position = value + a.Position);

				// Pseudo-static bodies can only have their position set on spawn.
			    if (controllingBody != null && !selfUpdate &&
					(controllingBody.BodyType != RigidBodyTypes.PseudoStatic || !isSpawnPositionSet))
			    {
					controllingBody.Position = value.ToJVector();
			    }

				isSpawnPositionSet = true;
			}
		}

		public virtual quat Orientation
		{
			get => orientation;
			set
			{
				Debug.Assert(!(controllingBody != null && controllingBody.BodyType == RigidBodyTypes.Static && 
					isSpawnOrientationSet), "For static entities (i.e. entities with a static controlling body), " +
					"orientation can only be set on spawn.");

				orientation = value;
				attachments.ForEach(a => a.Target.Orientation = value * a.Orientation);

				// Pseudo-static bodies (like moving platforms) shouldn't have position set directly.
				if (controllingBody != null && !selfUpdate &&
					(controllingBody.BodyType != RigidBodyTypes.PseudoStatic || !isSpawnOrientationSet))
			    {
				    controllingBody.Orientation = value.ToJMatrix();
			    }

				isSpawnOrientationSet = true;
			}
		}

		private void Attach(EntityAttachmentTypes attachmentType, ITransformable3D target, vec3? nullablePosition,
		    quat? nullableOrientation)
		{
			// The "a" prefix stands for "attachment".
			vec3 aPosition = nullablePosition ?? vec3.Zero;
			quat aOrientation = nullableOrientation ?? quat.Identity;
			quat effectiveOrientation = orientation * aOrientation;

			attachments.Add(new EntityAttachment(attachmentType, target, aPosition, aOrientation));
			target.SetTransform(position + aPosition * effectiveOrientation, effectiveOrientation);
		}
		
		protected Model CreateModel(Scene scene, string filename, bool shouldAttach = true, vec3? position = null,
			quat? orientation = null)
		{
			return CreateModel(scene, ContentCache.GetMesh(filename), shouldAttach, position, orientation);
		}

		protected Model CreateModel(Scene scene, Mesh mesh, bool shouldAttach = true, vec3? position = null,
			quat? orientation = null)
		{
			Debug.Assert(mesh != null, "Can't create a model with a null mesh.");

			Model model = new Model(mesh);
			scene.Renderer.Add(model);

			if (shouldAttach)
			{
				Attach(EntityAttachmentTypes.Model, model, position, orientation);
			}

			return model;
		}

		protected Sensor CreateSensor(Scene scene, Shape3D shape = null, SensorGroups group = SensorGroups.None,
			SensorTypes type = SensorTypes.Entity, vec3? position = null, quat? orientation = null)
		{
			var sensor = new Sensor(type, this, (int)group, shape);
			
			Attach(EntityAttachmentTypes.Sensor, sensor, position, orientation);
			scene.Space.Add(sensor);

			return sensor;
		}

		protected RigidBody CreateBody(Scene scene, Shape shape, RigidBodyTypes bodyType = RigidBodyTypes.Dynamic,
			RigidBodyFlags flags = RigidBodyFlags.None, bool isControlling = true, bool shouldAttach = true,
			vec3? position = null, quat? orientation = null)
		{
			Debug.Assert(shape != null, "Can't create a body with a null shape.");
			Debug.Assert(!isControlling || controllingBody == null, "Controlling body is already set.");
			Debug.Assert(isControlling || bodyType != RigidBodyTypes.Static, "Can't create a non-controlling " +
				"static body.");

			RigidBody body = new RigidBody(shape, bodyType, flags);
			body.Tag = this;

            // Note that the controlling body is intentionally not attached as a regular attachment. Doing so would
            // complicate transform sets by that body.
			scene.World.AddBody(body);

		    if (isControlling)
		    {
				if (isSpawnPositionSet)
				{
					body.Position = this.position.ToJVector();
				}

				if (isSpawnOrientationSet)
				{
					body.Orientation = this.orientation.ToJMatrix();
				}

			    controllingBody = body;
		    }
		    else if (shouldAttach)
		    {
			    Attach(EntityAttachmentTypes.Body, new TransformableBody(body), position, orientation);
		    }

			return body;
		}

		protected void RemoveSensor(Sensor sensor = null)
		{
			var space = Scene.Space;

			// If a sensor is given, only that sensor is removed.
			if (sensor != null)
			{
				Debug.Assert(attachments.Exists(a => a.Target == sensor), "Given sensor isn't attached.");

				space.Remove(sensor);

				for (int i = attachments.Count - 1; i >= 0; i--)
				{
					if (attachments[i].Target == sensor)
					{
						attachments.RemoveAt(i);

						return;
					}
				}

				return;
			}

			// If no sensor is given, the first attached sensor is removed.
			for (int i = attachments.Count - 1; i >= 0; i--)
			{
				Debug.Assert(attachments.Exists(a => a.AttachmentType == EntityAttachmentTypes.Sensor),
					"No sensors attached.");

				var attachment = attachments[i];

				if (attachment.AttachmentType == EntityAttachmentTypes.Sensor)
				{
					space.Remove((Sensor)attachment.Target);
					attachments.RemoveAt(i);

					return;
				}
			}
		}

		public virtual void Initialize(Scene scene, JToken data)
		{
			Scene = scene;

			if (data == null)
			{
				return;
			}

			// Parse ID.
			if (data.TryParse("Id", out int id))
			{
				Id = id;
			}

			// Parse handles.
			var hToken = data["Handle"];
			var hListToken = data["Handles"];

			Debug.Assert(hToken == null || hListToken == null, "Duplicate entity handle blocks. Use either Handle " +
				"(for singular handles) or Handles (for multiple handles).");

			if (hToken != null || hListToken != null)
			{
				handles = new List<EntityHandle>();

				if (hToken != null)
				{
					handles.Add(new EntityHandle(hToken));
				}
				else
				{
					foreach (var token in hListToken.Children())
					{
						handles.Add(new EntityHandle(token));
					}
				}
			}
		}

		public void ResolveHandles(Scene scene)
		{
			// Managing handles in this way simplifies extending classes (since they don't need to manually track
			// handles).
			if (handles != null && handles.Count > 0)
			{
				ResolveHandles(scene, handles);

				// Once handles have been resolved to actual entity references, handles aren't needed anymore.
				handles = null;
			}
		}

		protected virtual void ResolveHandles(Scene scene, List<EntityHandle> handles)
		{
		}

		public void SetTransform(vec3 position, quat orientation)
		{
			Position = position;
			Orientation = orientation;
		}

		public virtual bool OnContact(Entity entity, RigidBody body, vec3 p, vec3 normal, float penetration)
		{
			return true;
		}

		// Note that for this callback, the point is the position on the triangle, not the entity.
		public virtual bool OnContact(vec3 p, vec3 normal, vec3[] triangle, float penetration)
		{
			return true;
		}

		public virtual void Dispose()
		{
			foreach (var attachment in attachments)
			{
				var target = attachment.Target;

				switch (attachment.AttachmentType)
				{
					case EntityAttachmentTypes.Body:
						Scene.World.Remove(((TransformableBody)target).Body);

						break;

					case EntityAttachmentTypes.Model:
						Scene.Renderer.Remove((Model)target);

						break;

					case EntityAttachmentTypes.Sensor:
						Scene.Space.Remove((Sensor)target);

						break;
				}
			}

			if (controllingBody != null)
			{
				Scene.World.Remove(controllingBody);
			}
		}

		public virtual void Update(float dt)
		{
			Components.Update(dt);

			// Static bodies can't move, so there'd be no point updating transform.
		    if (controllingBody != null && controllingBody.BodyType != RigidBodyTypes.Static)
		    {
		        selfUpdate = true;
		        Position = controllingBody.Position.ToVec3();
				Orientation = controllingBody.Orientation.ToQuat();
		        selfUpdate = false;
		    }
		}
	}
}
