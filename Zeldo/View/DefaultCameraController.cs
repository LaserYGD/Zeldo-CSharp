using Engine;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using Zeldo.Entities;

namespace Zeldo.View
{
	public class DefaultCameraController : CameraController3D
	{
		private Player player;
		private vec3 fixedOffset;
		
		public DefaultCameraController(Player player)
		{
			this.player = player;
		}

		public override void Initialize(Camera3D camera)
		{
			float angle = Properties.GetFloat("camera.fixed.angle");
			float distance = Properties.GetFloat("camera.fixed.distance");

			vec2 v = Utilities.Direction(angle);

			fixedOffset = new vec3(0, v.y, -v.x) * distance + new vec3(0, 0, 7);
			camera.Orientation = quat.FromAxisAngle(angle, vec3.UnitX);

			base.Initialize(camera);
		}

		public override void Update()
		{
			Camera.Position = player.Position + fixedOffset;
		}
	}
}
