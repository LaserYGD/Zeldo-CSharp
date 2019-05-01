using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Zeldo.Entities.Core
{
	public class EntityAttachment
	{
		public EntityAttachment(ITransformable3D target, vec3 position, quat orientation)
		{
			Target = target;
			Position = position;
			Orientation = orientation;
		}

		public ITransformable3D Target { get; }

		public vec3 Position { get; }
		public quat Orientation { get; }
	}
}
