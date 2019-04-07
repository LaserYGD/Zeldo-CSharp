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
	public class Camera : IReceiver, IPositionable3D, IOrientable, IDynamic
	{
		public Camera()
		{
			MessageHandles = new List<MessageHandle>();

			MessageSystem.Subscribe(this, CoreMessageTypes.Resize, (messageType, data, dt) =>
			{
				OnResize((ivec2)data);
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }

		public bool IsOrthographic { get; set; }

		public vec3 Position { get; set; }
		public quat Orientation { get; set; }
		public mat4 View { get; private set; }
		public mat4 Projection { get; private set; }

		private void OnResize(ivec2 dimensions)
		{
			Projection = IsOrthographic
				? mat4.Ortho(-12, 12, 8, -8, 0.1f, 100)
				: mat4.Zero;
		}

		public void Update(float dt)
		{
			View = new mat4(Orientation) * mat4.Translate(Position.x, Position.y, Position.z);
		}
	}
}
