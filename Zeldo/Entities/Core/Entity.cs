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
	public abstract class Entity : ITransformable3D, IDynamic, IDisposable
	{
		private quat orientation;
		private List<EntityAttachment> attachments;

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
		}
		
		protected ComponentCollection Components { get; }

		public EntityGroups Group { get; }

		public Scene Scene { get; protected set; }
		public RigidBody ControllingBody => controllingBody;

		public virtual vec3 Position
		{
			get => position;
			set
			{
				position = value;
				attachments.ForEach(a => a.Target.Position = value + a.Position);

			    if (controllingBody != null && !selfUpdate)
			    {
					controllingBody.Position = value.ToJVector();
			    }
			}
		}

		public quat Orientation
		{
			get => orientation;
			set
			{
				orientation = value;
				attachments.ForEach(a => a.Target.Orientation = value * a.Orientation);

			    if (controllingBody != null && !selfUpdate)
			    {
				    controllingBody.Orientation = value.ToJMatrix();
			    }
			}
		}

		// "Persistent" means that when the entity would normally be unloaded (via unloading its fragment), the entity
		// instead remains loaded. This allows the entity to continue updating even when not visible and far away.
		public bool IsPersistent { get; protected set; }

		public virtual void Dispose()
		{
			foreach (var attachment in attachments)
			{
				var target = attachment.Target;

				switch (attachment.AttachmentType)
				{
					case EntityAttachmentTypes.Body:
						Scene.World.RemoveBody(((TransformableBody)target).Body);

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
				Scene.World.RemoveBody(controllingBody);
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
		
		protected Model CreateModel(Scene scene, string filename, vec3? position = null, quat? orientation = null)
		{
			return CreateModel(scene, ContentCache.GetMesh(filename), position, orientation);
		}

		protected Model CreateModel(Scene scene, Mesh mesh, vec3? position = null, quat? orientation = null)
		{
			Debug.Assert(mesh != null, "Can't create a model with a null mesh.");

			Model model = new Model(mesh);

			Attach(EntityAttachmentTypes.Model, model, position, orientation);
			scene.Renderer.Add(model);

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
			bool isControlling = true, vec3? position = null, quat? orientation = null)
		{
			Debug.Assert(shape != null, "Can't create a body with a null shape.");
			Debug.Assert(!isControlling || controllingBody == null, "Controlling body is already set.");

			RigidBody body = new RigidBody(shape);
			body.BodyType = bodyType;
			body.Tag = this;

            // Note that the controlling body is intentionally not attached as a regular attachment. Doing so would
            // complicate transform sets by that body.
			scene.World.AddBody(body);

		    if (isControlling)
		    {
			    body.Position = this.position.ToJVector();
			    body.Orientation = this.orientation.ToJMatrix();
			    controllingBody = body;
		    }
		    else
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
		}

		public void SetTransform(vec3 position, quat orientation)
		{
			Position = position;
			Orientation = orientation;
		}

		public virtual bool OnContact(Entity entity, vec3 p, vec3 normal, float penetration)
		{
			return true;
		}

		// Note that for this callback, the point is the position on the triangle, not the entity.
		public virtual bool OnContact(vec3 p, vec3 normal, vec3[] triangle, float penetration)
		{
			return true;
		}

		public virtual void Update(float dt)
		{
			Components.Update(dt);

		    if (controllingBody != null)
		    {
		        selfUpdate = true;
		        Position = controllingBody.Position.ToVec3();

			    if (!controllingBody.IsRotationFixed)
			    {
				    Orientation = controllingBody.Orientation.ToQuat();
				}

		        selfUpdate = false;
		    }
		}
	}
}
