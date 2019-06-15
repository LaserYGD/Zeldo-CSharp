using Engine;
using Engine.Core._3D;
using Engine.Graphics._3D;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Zeldo
{
	public class SpiralStaircase : IPositionable3D
	{
		public SpiralStaircase(ModelBatch batch)
		{
			Mesh mesh = ContentCache.GetMesh("Castle/SpiralStep.obj");

			float height = mesh.Bounds.y;
			
			for (int i = 0; i < 10; i++)
			{
				Model model = new Model(mesh);
				model.Orientation = quat.FromAxisAngle(0.3f * i, vec3.UnitY);
				model.Position = new vec3(0, height * i, 0);

				batch.Add(model);
			}
		}

		// A spiral staircase's position is defined by its bottom-center.
		public vec3 Position { get; set; }

		public float InnerRadius { get; set; }
		public float OuterRadius { get; set; }
		public float Height { get; set; }

		// In this context, slope is defined as height per full rotation around the central axis (two pi).
		public float Slope { get; set; }
	}
}
