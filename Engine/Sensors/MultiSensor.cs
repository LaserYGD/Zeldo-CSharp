using System.Collections.Generic;
using Engine.Shapes._3D;
using GlmSharp;

namespace Engine.Sensors
{
	public class MultiSensor : Sensor
	{
		private vec3 position;
		private quat orientation;

		public MultiSensor(SensorTypes type, object owner, int group, Shape3D shape = null) :
			base(type, owner, group, true, shape)
		{
			Attachments = new List<ShapeAttachment>();
		}

		internal List<ShapeAttachment> Attachments { get; }
		
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
			Attachments.Add(new ShapeAttachment(shape, position, orientation));
		}

		private void RecomputeAttachments()
		{
			foreach (var attachment in Attachments)
			{
				var shape = attachment.Shape;

				shape.Position = position + Orientation * attachment.Position;
				shape.Orientation = orientation * attachment.Orientation;
			}
		}
	}
}
