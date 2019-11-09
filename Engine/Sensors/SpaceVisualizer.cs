using Engine.Core;
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
				case ShapeTypes3D.Box: primitives.Draw((Box)shape, Color.Cyan);
					break;
			}
		}
	}
}
