using System;
using System.Collections.Generic;
using Engine.Core;
using Engine.Core._3D;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.Physics;
using Engine.Shapes._2D;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Zeldo.Interfaces;
using Zeldo.Physics._2D;
using Zeldo.Sensors;

namespace Zeldo.Entities.Core
{
	public abstract class Entity : ITransformable3D, IDynamic, IDisposable
	{
		private vec3 position;
		private quat orientation;
		private List<EntityAttachment2D> attachments2D;
		private List<EntityAttachment3D> attachments3D;

		// This body is also used by actors (entities that use the 2D movement system).
		protected RigidBody controllingBody3D;

		// The entity's transform properties (Position and Orientation) also update attachments. Since the controlling
        // physics body is itself an attachment, this would cause bodies to set their own transforms again (which is
        // wasteful). Using this variable avoids that.
	    protected bool selfUpdate;

		protected Entity(EntityGroups group)
		{
			Group = group;
			orientation = quat.Identity;
			attachments2D = new List<EntityAttachment2D>();
			attachments3D = new List<EntityAttachment3D>();
			Components = new ComponentCollection();
		}
		
		protected ComponentCollection Components { get; }

		public EntityGroups Group { get; }

		public Scene Scene { get; protected set; }

		public virtual vec3 Position
		{
			get => position;
			set
			{
				position = value;

				foreach (var attachment in attachments2D)
				{
					var target = attachment.Target;

					// TODO: Rotate position offset by the current orientation.
					target.Position = value.swizzle.xz + attachment.Position;
					target.Elevation = value.y + attachment.Elevation;
				}

				attachments3D.ForEach(a => a.Target.Position = value + a.Position);

			    if (controllingBody3D != null && !selfUpdate)
			    {
				    controllingBody3D.Position = value.ToJVector();
			    }
			}
		}

		public quat Orientation
		{
			get => orientation;
			set
			{
				// TODO: Rotating 2D attachments here as well.
				orientation = value;
				attachments3D.ForEach(a => a.Target.Orientation = value * a.Orientation);

			    if (controllingBody3D != null && !selfUpdate)
			    {
				    controllingBody3D.Orientation = value.ToJMatrix();
			    }
			}
		}

		public virtual void Dispose()
		{
			foreach (var attachment in attachments2D)
			{
				var target = attachment.Target;

				switch (attachment.AttachmentType)
				{
					case EntityAttachmentTypes2D.Body:
						Scene.World2D.Remove((RigidBody2D)target);

						break;

					case EntityAttachmentTypes2D.Sensor:
						Scene.Space.Remove((Sensor)target);

						break;
				}
			}

			foreach (var attachment in attachments3D)
			{
				var target = attachment.Target;

				switch (attachment.AttachmentType)
				{
					case EntityAttachmentTypes3D.Body:
						Scene.World3D.RemoveBody(((TransformableBody)target).Body);

						break;

					case EntityAttachmentTypes3D.Model:
						Scene.ModelBatch.Remove((Model)target);

						break;
				}
			}

			if (controllingBody3D != null)
			{
				Scene.World3D.RemoveBody(controllingBody3D);
			}
		}

		private void Attach(EntityAttachmentTypes2D attachmentType, ITransformable2D target, vec2? nullablePosition,
			float elevation, float rotation)
		{
			vec2 aPosition = nullablePosition ?? vec2.Zero;

			attachments2D.Add(new EntityAttachment2D(attachmentType, target, aPosition, elevation, rotation));

			// TODO: Apply entity orientation to 2D attachments (i.e. extract flat rotation from the quaternion).
			float effectiveRotation = rotation;

			target.SetTransform(position.swizzle.xz + Utilities.Rotate(aPosition, effectiveRotation),
				position.y, effectiveRotation);
		}

		private void Attach(EntityAttachmentTypes3D attachmentType, ITransformable3D target, vec3? nullablePosition,
		    quat? nullableOrientation)
		{
			// The "a" prefix stands for "attachment".
			vec3 aPosition = nullablePosition ?? vec3.Zero;
			quat aOrientation = nullableOrientation ?? quat.Identity;
			quat effectiveOrientation = orientation * aOrientation;

			attachments3D.Add(new EntityAttachment3D(attachmentType, target, aPosition, aOrientation));
			target.SetTransform(position + aPosition * effectiveOrientation, effectiveOrientation);
		}
		
		protected Model CreateModel(Scene scene, string filename, vec3? position = null, quat? orientation = null)
		{
			Model model = new Model(filename);
			
			Attach(EntityAttachmentTypes3D.Model, model, position, orientation);
			scene.ModelBatch.Add(model);

			return model;
		}

		protected Sensor CreateSensor(Scene scene, Shape2D shape = null, SensorUsages usage = SensorUsages.None,
			float height = 1, vec2? position = null, float elevation = 0, float rotation = 0, bool enabled = true,
			SensorTypes type = SensorTypes.Entity)
		{
			Sensor sensor = new Sensor(type, usage, this, shape, height);
			sensor.IsEnabled = enabled;
			
			Attach(EntityAttachmentTypes2D.Sensor, sensor, position, elevation, rotation);
			scene.Space.Add(sensor);

			return sensor;
		}

		protected RigidBody CreateRigidBody3D(Scene scene, Shape shape, bool isControlling = true,
			bool isStatic = false, vec3? position = null, quat? orientation = null)
		{
			RigidBody body = new RigidBody(shape);
			body.IsStatic = isStatic;
			body.Tag = this;

            // Note that the contrlling body is intentionally not attached as a regular attachment. Doing so would
            // complicate transform sets by that body.
			scene.World3D.AddBody(body);

		    if (isControlling)
		    {
			    body.Position = this.position.ToJVector();
			    body.Orientation = this.orientation.ToJMatrix();
			    controllingBody3D = body;
		    }
		    else
		    {
			    Attach(EntityAttachmentTypes3D.Body, new TransformableBody(body), position, orientation);
		    }

			return body;
		}

		protected RigidBody2D CreateGroundBody(Scene scene, Shape2D shape, bool isStatic = false,
			vec2? position = null, float rotation = 0)
		{
			var body = new RigidBody2D(shape, isStatic);
			body.Position = Position.swizzle.xz;
			body.Elevation = Position.z;
			
			// Just like controlling 3D bodies, the ground body is intentionally not attached as a regular 2D
			// attachment.
			scene.World2D.Add(body);

			return body;
		}

		protected void RemoveSensor(Sensor sensor = null)
		{
			var space = Scene.Space;

			if (sensor != null)
			{
				space.Remove(sensor);

				for (int i = attachments2D.Count - 1; i >= 0; i--)
				{
					if (attachments2D[i].Target == sensor)
					{
						attachments2D.RemoveAt(i);

						return;
					}
				}

				return;
			}

			// If no specific sensor is given, the first sensor in the attachment list is removed.
			for (int i = attachments2D.Count - 1; i >= 0; i--)
			{
				var attachment = attachments2D[i];

				if (attachment.AttachmentType == EntityAttachmentTypes2D.Sensor)
				{
					space.Remove((Sensor)attachment.Target);
					attachments2D.RemoveAt(i);

					return;
				}
			}
		}

		public virtual void Initialize(Scene scene)
		{
			Scene = scene;
		}

		public void SetTransform(vec3 position, quat orientation)
		{
			Position = position;
			Orientation = orientation;
		}

		public virtual void OnCollision(Entity entity, vec3 point, vec3 normal)
		{
		}

		public void PlayAnimation(string animation)
		{
		}

		public virtual void Update(float dt)
		{
			Components.Update(dt);

		    if (controllingBody3D != null)
		    {
		        selfUpdate = true;
		        Position = controllingBody3D.Position.ToVec3();
		        Orientation = controllingBody3D.Orientation.ToQuat();
		        selfUpdate = false;
		    }
		}
	}
}
