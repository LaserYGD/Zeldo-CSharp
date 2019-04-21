using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.Shapes._2D;
using GlmSharp;
using Zeldo.Sensors;

namespace Zeldo.Entities.Core
{
	public abstract class Entity : ITransformable3D, IDynamic
	{
		private vec3 position;
		private List<Sensor> sensors;
		private List<DynamicComponent> components;

		protected Entity(EntityTypes type)
		{
			EntityType = type;
		}

		protected List<Sensor> Sensors => sensors ?? (sensors = new List<Sensor>());
		protected List<DynamicComponent> Components => components ?? (components = new List<DynamicComponent>());

		public EntityTypes EntityType { get; }

		public Scene Scene { get; set; }

		public vec3 Position
		{
			get => position;
			set
			{
				position = value;
				sensors?.ForEach(s => s.Position = value);
			}
		}

		public quat Orientation { get; set; }

		public virtual void Initialize()
		{
		}

		protected Sensor CreateSensor(Shape2D shape = null, bool enabled = true, int height = 1,
			SensorTypes type = SensorTypes.Entity)
		{
			Sensor sensor = new Sensor(type, this, shape, height);
			sensor.Position = position;
			sensor.Enabled = enabled;

			Sensors.Add(sensor);
			Scene.Space.Add(sensor);

			return sensor;
		}

		public void SetTransform(vec3 position, quat orientation)
		{
			Position = position;
			Orientation = orientation;
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

			sensors?.ForEach(s => s.Position = position);
		}
	}
}
