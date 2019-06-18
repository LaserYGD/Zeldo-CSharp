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
	public class Camera3D : IReceiver, ITransformable3D, IDynamic
	{
		private mat4 projection;
		private CameraController3D controller;

		private float orthoHalfWidth;
		private float orthoHalfHeight;

		private bool isOrthographic;

		public Camera3D()
		{
			Orientation = quat.Identity;

			MessageSystem.Subscribe(this, CoreMessageTypes.ResizeRender, (messageType, data, dt) =>
			{
				RecomputeProjection();
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }

		public float OrthoWidth
		{
			get => orthoHalfWidth * 2;
			set => orthoHalfWidth = value / 2;
		}

		public float OrthoHeight
		{
			get => orthoHalfHeight * 2;
			set => orthoHalfHeight = value / 2;
		}

		public float NearPlane { get; set; }
		public float FarPlane { get; set; }

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
		public mat4 ViewProjectionInverse { get; private set; }

		public CameraController3D Controller => controller;

		public void Dispose()
		{
			MessageSystem.Unsubscribe(this);
		}

		public void SetTransform(vec3 position, quat orientation)
		{
			Position = position;
			Orientation = orientation;
		}

		private void RecomputeProjection()
		{
			ivec2 dimensions = Resolution.RenderDimensions;

			projection = isOrthographic
				? mat4.Ortho(-orthoHalfWidth, orthoHalfWidth, -orthoHalfHeight, orthoHalfHeight, NearPlane, FarPlane)
				: mat4.PerspectiveFov(90, dimensions.x, dimensions.y, NearPlane, FarPlane);
		}

		public void Attach(CameraController3D controller)
		{
			this.controller = controller;

			controller.Initialize(this);
		}

		public void Update(float dt)
		{
			controller?.Update();

			mat4 view = new mat4(Orientation) * mat4.Translate(-Position.x, -Position.y, -Position.z);

			ViewProjection = projection * view;
			ViewProjectionInverse = ViewProjection.Inverse;
		}
	}
}
