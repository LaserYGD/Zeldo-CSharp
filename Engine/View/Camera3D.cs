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
			Orientation = quat.Identity;

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

			float aspectRatio = (float)dimensions.y / dimensions.x;

			projection = isOrthographic
				? mat4.Ortho(-4, 4, -3, 3, 0.1f, 100)
				: mat4.PerspectiveFov(90, dimensions.x, dimensions.y, 0.1f, 100);
		}

		public void Update(float dt)
		{
			mat4 view = new mat4(Orientation) * mat4.Translate(Position.x, Position.y, Position.z);
			//mat4 view = mat4.LookAt(Position, vec3.Zero, vec3.UnitY);

			ViewProjection = projection * view;
		}
	}
}
