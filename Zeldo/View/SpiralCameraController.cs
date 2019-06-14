using Engine;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using Zeldo.Entities;

namespace Zeldo.View
{
	public class SpiralCameraController : CameraController3D
	{
		private Player player;
		private vec3 fixedOffset;
		private quat tiltedView;

		public SpiralCameraController(Player player)
		{
			this.player = player;

			float angle = Properties.GetFloat("camera.fixed.angle");
			float distance = Properties.GetFloat("camera.fixed.distance");

			vec2 v = Utilities.Direction(angle);

			fixedOffset = new vec3(0, v.y, v.x) * distance;
			//tiltedView = quat.FromAxisAngle(-angle, vec3.UnitX);
		}

		// The camera follows the player around spiral staircases. Height is determined by the player's elevation,
		// while the central axis is used to orient the camera towards the staircases's center.
		public vec2 Axis { get; }

		public override void Update()
		{
			vec3 position = player.Position;
			float axisRotation = Utilities.Angle(position.swizzle.xz, Axis);
			quat axisOrientation = quat.FromAxisAngle(axisRotation, vec3.UnitY);

			Camera.Orientation = axisOrientation;
			Camera.Position = position + fixedOffset;
		}
	}
}
