using Engine.Interfaces._3D;
using GlmSharp;

namespace Zeldo
{
	public class SpiralStaircase : IPositionable3D
	{
		public SpiralStaircase()
		{
			/*
			Mesh mesh = ContentCache.GetMesh("Castle/SpiralStep.obj");

			float height = mesh.Bounds.y;
			
			for (int i = 0; i < 10; i++)
			{
				Model model = new Model(mesh);
				model.Orientation = quat.FromAxisAngle(0.3f * i, vec3.UnitY);
				model.Position = new vec3(0, height * i, 0);

				batch.Add(model);
			}
			*/
		}

		// A spiral staircase's position is defined by its bottom-center.
		public vec3 Position { get; set; }

		public float InnerRadius { get; set; }
		public float OuterRadius { get; set; }

		// In this context, slope is defined as height per radian.
		public float Slope { get; set; }

		// Whether the staircase ascends clockwise or counter-clockwise affects how velocity is applied to actors on
		// the staircase.
		public bool IsClockwise { get; set; }
	}
}
