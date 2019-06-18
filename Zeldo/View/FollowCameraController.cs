using Engine;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using Zeldo.Entities;

namespace Zeldo.View
{
	public class FollowCameraController : CameraController3D
	{
		private Player player;
		private vec3 fixedOffset;
		
		public FollowCameraController(Player player)
		{
			this.player = player;
		}

		public vec2? Axis { get; set; }

		public override void Initialize(Camera3D camera)
		{
			float angle = Properties.GetFloat("camera.fixed.angle");
			float distance = Properties.GetFloat("camera.fixed.distance");

			vec2 v = Utilities.Direction(angle);

			fixedOffset = new vec3(0, v.y, v.x) * distance;

			camera.OrthoWidth = Properties.GetFloat("camera.ortho.width");
			camera.OrthoHeight = Properties.GetFloat("camera.ortho.height");
			camera.NearPlane = Properties.GetFloat("camera.near.plane");
			camera.FarPlane = Properties.GetFloat("camera.far.plane");

			base.Initialize(camera);

			ResetOrientation();
		}

		// This function is used when leaving a spiral staircase to ensure the camera locks back into its default,
		// axis-aligned orientation.
		public void ResetOrientation()
		{
			Camera.Orientation = new quat(mat4.LookAt(fixedOffset, vec3.Zero, vec3.UnitY));
		}

		public override void Update()
		{
			vec3 p = player.Position;

			if (Axis != null)
			{
				float rotation = Utilities.Angle(p.swizzle.xz, Axis.Value) + Constants.PiOverTwo;

				vec3 eye = p + quat.FromAxisAngle(-rotation, vec3.UnitY) * fixedOffset;

				Camera.Position = eye;
				Camera.Orientation = new quat(mat4.LookAt(eye, p, vec3.UnitY));
			}
			else
			{
				Camera.Position = p + fixedOffset;
			}
		}
	}
}
