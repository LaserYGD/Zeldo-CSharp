using Engine.Interfaces._3D;
using GlmSharp;

namespace Zeldo.Entities.Core
{
	public class EntityAttachment
	{
		public EntityAttachment(EntityAttachmentTypes attachmentType, ITransformable3D target, vec3 position,
			quat orientation)
		{
			AttachmentType = attachmentType;
			Target = target;
			Position = position;
			Orientation = orientation;
		}

		public EntityAttachmentTypes AttachmentType { get; }
		public ITransformable3D Target { get; }

		public vec3 Position { get; }
		public quat Orientation { get; }
	}
}
