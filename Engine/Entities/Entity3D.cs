using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Entities
{
	public class Entity3D : ITransformable3D, IDynamic
	{
		private vec3 position;
		private quat orientation;
		private List<Attachment> attachments;

		protected Entity3D()
		{
			attachments = new List<Attachment>();
		}

		public virtual vec3 Position
		{
			get => position;
			set
			{
				position = value;
				RecomputeAttachments();
			}
		}

		public virtual quat Orientation
		{
			get => orientation;
			set
			{
				orientation = value;
				RecomputeAttachments();
			}
		}

		public Scene Scene { get; set; }

		protected void Attach(ITransformable3D target)
		{
			Attach(target, vec3.Zero, quat.Zero);
		}

		protected void Attach(ITransformable3D target, vec3 position, quat orientation)
		{
			attachments.Add(new Attachment(target, position, orientation));
		}

		public void SetTransform(vec3 position, quat orientation)
		{
			this.position = position;
			this.orientation = orientation;

			RecomputeAttachments();
		}

		private void RecomputeAttachments()
		{
			foreach (Attachment attachment in attachments)
			{
				var target = attachment.Target;
				target.Position = position + orientation * attachment.Position;
				target.Orientation = orientation * attachment.Orientation;
			}
		}

		public virtual void Update(float dt)
		{
		}
	}
}
