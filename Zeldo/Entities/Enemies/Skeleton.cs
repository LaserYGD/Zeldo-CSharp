using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Entities;
using Engine.Interfaces._2D;
using Engine.Sensors;
using Engine.Shapes._3D;
using GlmSharp;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Enemies
{
	public class Skeleton : Entity3D, ISensitive, ITargetable
	{
		public Skeleton()
		{
			MaxHealth = 8;
			Health = MaxHealth;
			Box = new Box(0.6f, 1.8f, 0.6f);
		}

		public int Health { get; set; }
		public int MaxHealth { get; set; }

		public Box Box { get; }

		public void OnSense(SensorTypes sensorType, object target)
		{
		}

		public void OnSeparate(SensorTypes sensorType, object target)
		{
		}

		public void OnHit(int damage, int power, float angle, vec2 direction)
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
		}
	}
}
