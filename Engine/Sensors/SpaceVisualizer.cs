using Engine.Graphics._3D;
using Engine.Shapes._3D;
using Engine.View;

namespace Engine.Sensors
{
	public class SpaceVisualizer
	{
		private PrimitiveRenderer3D primitives;
		private Space space;

		public SpaceVisualizer(Camera3D camera, Space space)
		{
			this.space = space;

			primitives = new PrimitiveRenderer3D(camera, 10000, 1000);
		}

		public void Draw()
		{
			foreach (var sensor in space.Sensors)
			{
				if (sensor.IsCompound)
				{
					((MultiSensor)sensor).Attachments.ForEach(a => Draw(a.Shape));
				}
				else
				{
					Draw(sensor.Shape);
				}
			}

			primitives.Flush();
		}

		private void Draw(Shape3D shape)
		{
			var p = shape.Position;

			switch (shape.ShapeType)
			{
				/*
				case ShapeTypes2D.Circle:
					Circle circle = (Circle)shape;
					vec3[] points = new vec3[32];

					float increment = Constants.TwoPi / points.Length;

					for (int i = 0; i < points.Length; i++)
					{
						vec2 v = circle.Position + Utilities.Direction(rotation + increment * i) * circle.Radius;
						points[i] = new vec3(v.x, elevation - height / 2, v.y);
					}

					for (int i = 0; i < points.Length; i++)
					{
						vec3 point = points[i];
						vec3 next = points[(i + 1) % points.Length];
						vec3 upper = point + new vec3(0, height, 0);
						vec3 upperNext = next + new vec3(0, height, 0);

						primitives.DrawLine(point, next, Color.Cyan);
						primitives.DrawLine(point, upper, Color.Cyan);
						primitives.DrawLine(upper, upperNext, Color.Cyan);
					}

					break;

				case ShapeTypes2D.Rectangle:
					Rectangle rect = (Rectangle)shape;
					Box box = new Box(rect.Width, height, rect.Height);
					box.Position = new vec3(p.x, elevation, p.y);
					box.Orientation = quat.FromAxisAngle(rotation, vec3.UnitY);

					primitives.Draw(box, Color.Cyan);

					break;
				*/
			}
		}
	}
}
