using GlmSharp;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Core
{
	public class EntityAttachment2D
	{
		public EntityAttachment2D(EntityAttachmentTypes2D attachmentType, ITransformable2D target, vec2 position,
			float rotation)
		{
			AttachmentType = attachmentType;
			Target = target;
			Position = position;
			Rotation = rotation;
		}

		public EntityAttachmentTypes2D AttachmentType { get; }
		public ITransformable2D Target { get; }

		public vec2 Position { get; }

		public float Rotation { get; }
	}
}
