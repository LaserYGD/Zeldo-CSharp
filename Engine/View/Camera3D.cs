using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.Messaging;
using GlmSharp;

namespace Engine.View
{
	public class Camera3D : IReceiver, IPositionable3D, IOrientable, IDynamic
	{
		private mat4 projection;

		private bool isOrthographic;

		public Camera3D()
		{
			MessageHandles = new List<MessageHandle>();

			MessageSystem.Subscribe(this, CoreMessageTypes.Resize, (messageType, data, dt) =>
			{
				RecomputeProjection();
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }

		public bool IsOrthographic
		{
			get => isOrthographic;
			set
			{
				isOrthographic = value;
				RecomputeProjection();
			}
		}

		public vec3 Position { get; set; }
		public quat Orientation { get; set; }
		public mat4 ViewProjection { get; private set; }

		private void RecomputeProjection()
		{
			ivec2 dimensions = Resolution.WindowDimensions;

			projection = isOrthographic
				? mat4.Ortho(-12, 12, 8, -8, 0.1f, 100)
				: mat4.PerspectiveFov((float)Math.PI, dimensions.x, dimensions.y, 0.1f, 100);
		}

		public void Update(float dt)
		{
			mat4 view = new mat4(Orientation) * mat4.Translate(Position.x, Position.y, Position.z);

			ViewProjection = view * projection;
		}
	}
}
