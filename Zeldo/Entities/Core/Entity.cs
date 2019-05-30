using System;
using System.Collections.Generic;
using Engine.Core;
using Engine.Core._3D;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.Physics;
using Engine.Shapes._2D;
using GlmSharp;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Zeldo.Sensors;

namespace Zeldo.Entities.Core
{
	public abstract class Entity : ITransformable3D, IDynamic, IDisposable
	{
		private vec3 position;
		private quat orientation;
		private List<Sensor> sensors;
		private List<DynamicComponent> components;
		private List<EntityAttachment> attachments;
		private RigidBody controllingBody;

		// The entity's transform properties (Position and Orientation) also update attachments. Since the controlling
        // physics body is itself an attachment, this would cause bodies to set their own transforms again (which is
        // wasteful). Using this variable avoids that.
	    private bool selfUpdate;

		protected Entity(EntityGroups group)
		{
			Group = group;
			attachments = new List<EntityAttachment>();
		}

		protected List<Sensor> Sensors => sensors ?? (sensors = new List<Sensor>());
		protected List<DynamicComponent> Components => components ?? (components = new List<DynamicComponent>());

		public EntityGroups Group { get; }

		public Scene Scene { get; protected set; }

		public vec3 Position
		{
			get => position;
			set
			{
				position = value;
				sensors?.ForEach(s => s.Position = value);
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

		public RigidBody ControllingBody => controllingBody;

		protected void Attach(EntityAttachmentTypes attachmentType, ITransformable3D target)
		{
			Attach(attachmentType, target, vec3.Zero, quat.Identity);
		}

		protected void Attach(EntityAttachmentTypes attachmentType, ITransformable3D target, vec3 position,
		    quat orientation)
		{
			attachments.Add(new EntityAttachment(attachmentType, target, position, orientation));
		}

		protected Model CreateModel(Scene scene, string filename)
		{
			Model model = new Model(filename);
			model.SetTransform(position, orientation);

			Attach(EntityAttachmentTypes.Model, model);
			scene.ModelBatch.Add(model);

			return model;
		}

		protected Sensor CreateSensor(Scene scene, Shape2D shape = null, bool enabled = true, int height = 1,
			SensorTypes type = SensorTypes.Entity)
		{
			Sensor sensor = new Sensor(type, this, shape, height);
			sensor.Position = position;
			sensor.IsEnabled = enabled;

			Sensors.Add(sensor);
			scene.Space.Add(sensor);

			return sensor;
		}

		protected RigidBody CreateRigidBody(Scene scene, Shape shape, bool controlling = true)
		{
			RigidBody body = new RigidBody(shape);
		    body.Position = position.ToJVector();
		    body.Orientation = orientation.ToJMatrix();
			body.Tag = this;

            // Note that the contrlling body is intentionally not attached as a regular attachment. Doing so would
            // complicate transform sets by that body.
			scene.World.AddBody(body);

		    if (controlling)
		    {
			    controllingBody = body;
		    }

			return body;
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

		public virtual void Dispose()
		{
			sensors?.ForEach(Scene.Space.Remove);

			foreach (var attachment in attachments)
			{
				var target = attachment.Target;

				switch (attachment.AttachmentType)
				{
					case EntityAttachmentTypes.Model:
						Scene.ModelBatch.Remove((Model)target);
						break;
				}
			}

		    if (controllingBody != null)
		    {
		        Scene.World.RemoveBody(controllingBody);
		    }
		}

		public virtual void Update(float dt)
		{
			if (components != null)
			{
				for (int i = components.Count - 1; i >= 0; i--)
				{
					var component = components[i];
					component.Update(dt);

					if (component.Complete)
					{
						components.RemoveAt(i);
					}
				}
			}

		    if (controllingBody != null)
		    {
		        selfUpdate = true;
		        Position = controllingBody.Position.ToVec3();
		        Orientation = controllingBody.Orientation.ToQuat();
		        selfUpdate = false;
		    }
		}
	}
}
