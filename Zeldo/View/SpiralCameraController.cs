using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using Zeldo.Entities;

namespace Zeldo.View
{
	public class SpiralCameraController : CameraController3D
	{
		private Player player;
		private vec2 axis;

		public SpiralCameraController(Player player, vec2 axis)
		{
			this.player = player;
			this.axis = axis;
		}

		public override void Update()
		{
			vec3 position = player.Position;

			float axisRotation = Utilities.Angle(position.swizzle.xz, axis);

			Camera.Orientation = quat.FromAxisAngle(axisRotation, vec3.UnitY);
			Camera.Position = vec3.Zero;
		}
	}
}
