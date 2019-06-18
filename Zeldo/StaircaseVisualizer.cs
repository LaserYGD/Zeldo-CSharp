using System.Collections.Generic;
using Engine.Core;
using Engine.Graphics._3D;
using Engine.Utility;
using Engine.View;
using GlmSharp;

namespace Zeldo
{
	public class StaircaseVisualizer
	{
		private PrimitiveRenderer3D primitives;
		private List<vec3> points;

		public StaircaseVisualizer(Camera3D camera, SpiralStaircase staircase, float stepHeight, float stepCount,
			float stepSpread)
		{
			primitives = new PrimitiveRenderer3D(camera, 20000, 2000);
			points = new List<vec3>();

			float rotation = staircase.Rotation;

			for (int i = 0; i < stepCount; i++)
			{
				float angle1 = stepSpread * i + rotation;
				float angle2 = stepSpread * (i + 1) + rotation;
				float baseY = stepHeight * i;
				float innerRadius = staircase.InnerRadius;
				float outerRadius = staircase.OuterRadius;

				vec2 v1 = Utilities.Direction(angle1);
				vec2 v2 = Utilities.Direction(angle2);
				vec3 d1 = new vec3(v1.x, 0, v1.y);
				vec3 d2 = new vec3(v2.x, 0, v2.y);
				vec3 bottom = new vec3(0, baseY, 0);
				vec3 top = new vec3(0, baseY + stepHeight, 0);

				vec3[] step =
				{
					d1 * innerRadius + bottom,
					d1 * outerRadius + bottom,
					d1 * innerRadius + top,
					d1 * outerRadius + top,
					d2 * innerRadius + top,
					d2 * outerRadius + top
				};

				for (int j = 0; j < step.Length; j++)
				{
					step[j] += staircase.Position;
				}

				int[] indices = { 0, 1, 2, 3, 4, 5, 0, 2, 1, 3, 2, 4, 3, 5 };

				foreach (int index in indices)
				{
					points.Add(step[index]);
				}
			}
		}

		public void Draw()
		{
			for (int i = 0; i < points.Count; i += 2)
			{
				primitives.DrawLine(points[i], points[i + 1], Color.Green);
			}

			primitives.Flush();
		}
	}
}
