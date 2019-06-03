﻿using Engine;
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
			camera.Orientation = new quat(mat4.LookAt(fixedOffset, vec3.Zero, vec3.UnitY));

			base.Initialize(camera);
		}

		public override void Update()
		{
			Camera.Position = player.Position + fixedOffset;
		}
	}
}