using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces._2D;
using Engine.Shapes._2D;
using Engine.Shapes._3D;
using GlmSharp;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;
using Zeldo.Sensors;

namespace Zeldo.Entities.Enemies
{
	public class Skeleton : Entity, ITargetable
	{
		private Sensor sensor;

		public Skeleton() : base(EntityTypes.Enemy)
		{
			MaxHealth = 20;
			Health = MaxHealth;
			Box = new Box(0.6f, 1.8f, 0.6f);

			Circle circle = new Circle(0.4f);
			sensor = new Sensor(SensorTypes.Entity, this, circle);
			Sensors.Add(sensor);
		}

		public int Health { get; set; }
		public int MaxHealth { get; set; }

		public Box Box { get; }
		public Sensor Sensor => sensor;

		public void OnHit(int damage, int power, float angle, vec2 direction, object source)
		{
			Health -= damage;

			if (Health <= 0)
			{
				Health = 0;
				OnDeath();
			}
		}

		private void OnDeath()
		{
		}

		public override void Update(float dt)
		{
			Box.Position = Position;

			base.Update(dt);
		}
	}
}
