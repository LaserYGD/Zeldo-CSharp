using System.Collections.Generic;
using Engine.Shapes._3D;
using GlmSharp;

namespace Zeldo.Sensors
{
	public class CompoundSensor : Sensor
	{
		private vec3 position;
		private quat orientation;

		public CompoundSensor(SensorTypes type, SensorUsages usage, object parent) : base(type, usage, parent)
		{
			Attachments = new List<CompoundAttachment>();
			IsCompound = true;
		}

		public List<CompoundAttachment> Attachments { get; }
		
		public override vec3 Position
		{
			get => position;
			set
			{
				position = value;
				RecomputeAttachments();
			}
		}

		public override quat Orientation
		{
			get => orientation;
			set
			{
				orientation = value;
				RecomputeAttachments();
			}
		}

		public override void SetTransform(vec3 position, quat orientation)
		{
			this.position = position;
			this.orientation = orientation;

			RecomputeAttachments();
		}

		public void Attach(Shape3D shape, vec3 position, quat orientation)
		{
			Attachments.Add(new CompoundAttachment(shape, position, orientation));
		}

		private void RecomputeAttachments()
		{
			foreach (var attachment in Attachments)
			{
				var shape = attachment.Shape;
				var localOrientation = attachment.Orientation;

				shape.Position = position + attachment.Position * localOrientation;
				shape.Orientation = orientation * localOrientation;
			}
		}
	}
}
