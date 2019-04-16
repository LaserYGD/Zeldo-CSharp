﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces._2D;
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
			MaxHealth = 8;
			Health = MaxHealth;
			Box = new Box(0.6f, 1.8f, 0.6f);
		}

		public int Health { get; set; }
		public int MaxHealth { get; set; }

		public Box Box { get; }

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
		}
	}
}