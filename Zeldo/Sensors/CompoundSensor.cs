using System.Collections.Generic;
using Engine.Shapes._2D;
using Engine.Utility;
using GlmSharp;

namespace Zeldo.Sensors
{
	public class CompoundSensor : Sensor
	{
		private float rotation;

		private vec2 position;

		public CompoundSensor(SensorTypes type, SensorUsages usage, object parent) : base(type, usage, parent)
		{
			Attachments = new List<CompoundAttachment>();
			IsCompound = true;
		}

		public List<CompoundAttachment> Attachments { get; }

		public override float Rotation
		{
			get => rotation;
			set
			{
				rotation = value;
				RecomputeAttachments();
			}
		}

		public override float Elevation { get; set; }

		public override vec2 Position
		{
			get => position;
			set
			{
				position = value;
				RecomputeAttachments();
			}
		}

		public override void SetTransform(vec2 position, float elevation, float rotation)
		{
			this.position = position;
			this.rotation = rotation;

			Elevation = elevation;
			RecomputeAttachments();
		}

		public void Attach(Shape2D shape, float height, vec2 position, float elevation, float rotation = 0)
		{
			Attachments.Add(new CompoundAttachment(shape, height, position, elevation, rotation));
		}

		private void RecomputeAttachments()
		{
			foreach (var attachment in Attachments)
			{
				var shape = attachment.Shape;
				shape.Position = position + Utilities.Rotate(attachment.Position, rotation);
				shape.Rotation = attachment.Rotation + rotation;
			}
		}
	}
}
