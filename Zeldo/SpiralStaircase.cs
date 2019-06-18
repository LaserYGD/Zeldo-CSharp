using Engine.Interfaces._2D;
using Engine.Interfaces._3D;
using Engine.Shapes._2D;
using GlmSharp;
using Zeldo.Entities.Core;
using Zeldo.Sensors;

namespace Zeldo
{
	public class SpiralStaircase : IPositionable3D, IRotatable
	{
		private CompoundSensor sensor;

		private float height;

		public SpiralStaircase(float height)
		{
			this.height = height;
		}

		// A spiral staircases's position is defined by its bottom-center.
		public vec3 Position { get; set; }

		public float InnerRadius { get; set; }
		public float OuterRadius { get; set; }

		// In this context, slope is defined as height per radian.
		public float Slope { get; set; }
		public float Rotation { get; set; }

		// Whether the staircase ascends clockwise or counter-clockwise affects how velocity is applied to actors on
		// the staircase.
		public bool IsClockwise { get; set; }

		public void Initialize(Scene scene)
		{
			const float RectHeight = 2;
			const float Overlap = 0.2f;

			float r = OuterRadius;

			vec2 p = new vec2(r / 2, -r / 2);

			var rect1 = new Rectangle(r, r);
			var rect2 = new Rectangle(r, r);
			var circle = new Circle(OuterRadius);

			// It's assumed that the staircase will be initialized before the transform is set, and that the staircase
			// won't move once created.
			sensor = new CompoundSensor(SensorTypes.Zone, SensorUsages.Control, this);
			sensor.Attach(rect1, RectHeight, p, height);
			sensor.Attach(rect2, RectHeight, new vec2(p.x, -p.y), 0);
			sensor.Attach(circle, height - (RectHeight - Overlap), vec2.Zero, height / 2);
			sensor.OnSense = (sensorType, parent) =>
			{
				Actor actor = (Actor)parent;
				actor.OnSpiralStaircaseEnter(this);
			};

			sensor.OnSeparate = (sensorType, parent) =>
			{
				Actor actor = (Actor)parent;
				actor.OnSpiralStaircaseLeave();
			};

			scene.Space.Add(sensor);
		}

		public void SetTransform(vec3 position, float rotation)
		{
			Position = position;
			Rotation = rotation;
			sensor.SetTransform(position.swizzle.xz, position.y, rotation);
		}
	}
}
