using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core._3D;
using Engine.Graphics._3D;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.View;
using GlmSharp;

namespace Zeldo
{
	public class SkeletalTester : IDynamic
	{
		private Model model;

		public SkeletalTester()
		{
			model = new Model("Cube.dae");
			Batch = new ModelBatch(10000, 1000);
			Batch.Add(model);
		}

		public ModelBatch Batch { get; }

		public void Update(float dt)
		{
			model.Orientation *=
				quat.FromAxisAngle(dt / 2, vec3.UnitX) *
				quat.FromAxisAngle(dt / 3, vec3.UnitY);
		}
	}
}
