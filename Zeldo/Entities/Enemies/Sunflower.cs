using System;
using System.Collections.Generic;
using Engine;
using Engine.Shapes._2D;
using Engine.Utility;
using GlmSharp;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Enemies
{
	public class Sunflower : Enemy
	{
		private static float chargeLimit;
		private static float chargeRate;

		static Sunflower()
		{
			chargeLimit = Properties.GetFloat("sunflower.charge.limit");
			chargeRate = Properties.GetFloat("sunflower.charge.rate");
		}

		private float sunlightCharge;

		private bool isCharging;

		private List<Groot> linkedTrees;
		private List<SunflowerTendril> tendrils;

		public Sunflower()
		{
			linkedTrees = new List<Groot>();
		}

		public override void Initialize(Scene scene)
		{
			CreateModel(scene, "Sunflower.dae");

			base.Initialize(scene);
		}

		public override void OnHit(int damage, int knockback, float angle, vec2 direction, object source)
		{
			if (isCharging)
			{
				// Interrupt charging by playing an animation and disrupting tendrils linking to trees

				isCharging = false;
			}
		}

		protected override void OnDeath()
		{
			linkedTrees.ForEach(l => l.PoweredBySunlight = false);
		}

		public override void Update(float dt)
		{
			// Behavior: seek out patches of sunlight, then plan and start to absorb (and channel) strength to nearby
			// plant-based enemies. If all other enemies are killed, it breaks its focus and starts fighting you
			// directly. With enough charge, it can even fire a sunlight laser before becoming staggered and returning
			// to its default state.
			if (isCharging)
			{
				sunlightCharge += chargeRate * dt;
				sunlightCharge = Math.Min(sunlightCharge, chargeRate);
			}
			else
			{
				Rectangle[] windows = (Rectangle[])Scene.UserData["Windows"];
				Rectangle[] patches = SunflowerHelper.ProjectSunlight(windows, Scene.ModelBatch.LightDirection, 0);
				Rectangle target = Utilities.Closest(patches, Position.swizzle.xz);

				// Move to target location (using a navigation system)
				// Once reached, play an animation and begin gather sunlight
			}

			base.Update(dt);
		}
	}
}
