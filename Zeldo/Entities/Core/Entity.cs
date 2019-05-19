using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core;
using Engine.Core._3D;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.Physics;
using Engine.Shapes._2D;
using GlmSharp;
using Jitter.Dynamics;
using Zeldo.Sensors;

namespace Zeldo.Entities.Core
{
	public abstract class Entity : ITransformable3D, IDynamic
	{
		private vec3 position;
		private quat orientation;
		private List<Sensor> sensors;
		private List<DynamicComponent> components;
		private List<EntityAttachment> attachments;

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
			}
		}

		public quat Orientation
		{
			get => orientation;
			set
			{
				orientation = value;
				attachments.ForEach(a => a.Target.Orientation = value * a.Orientation);
			}
		}

		protected void Attach(ITransformable3D target)
		{
			Attach(target, vec3.Zero, quat.Identity);
		}

		protected void Attach(ITransformable3D target, vec3 position, quat orientation)
		{
			attachments.Add(new EntityAttachment(target, position, orientation));
		}

		protected Model CreateModel(Scene scene, string filename)
		{
			Model model = new Model(filename);
			model.SetTransform(position, orientation);

			Attach(model);
			scene.ModelBatch.Add(model);

			return model;
		}

		protected Sensor CreateSensor(Scene scene, Shape2D shape = null, bool enabled = true, int height = 1,
			SensorTypes type = SensorTypes.Entity)
		{
			Sensor sensor = new Sensor(type, this, shape, height);
			sensor.Position = position;
			sensor.Enabled = enabled;

			Sensors.Add(sensor);
			scene.Space.Add(sensor);

			return sensor;
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

			foreach (EntityAttachment attachment in attachments)
			{

			}

			sensors?.ForEach(s => s.Position = position);
		}
	}
}
