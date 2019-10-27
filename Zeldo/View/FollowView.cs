using System.Collections.Generic;
using Engine;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using Zeldo.Entities;
using Zeldo.Entities.Player;
using Zeldo.Settings;

namespace Zeldo.View
{
	public class FollowView : CameraController3D, IReceiver
	{
		private const float AimDivisor = 10000f;
		
		// Shifting the camera upward by a small amount gives a better view of objects above the player without
		// negatively affecting visibility of the player. Feels a bit better than a pure centered camera.
		private const float Shift = 1;

		private PlayerCharacter player;
		private ControlSettings settings;

		private float followDistance;
		private float pitch;
		private float yaw;
		private float maxPitch;
		
		public FollowView(Camera3D camera, PlayerCharacter player, ControlSettings settings) : base(camera)
		{
			this.player = player;
			this.settings = settings;

			player.FollowView = this;

			followDistance = Properties.GetFloat("view.follow.distance");
			maxPitch = Properties.GetFloat("view.max.pitch");

			// TODO: These values should probably be set in a different spot (since they're not exclusive to this view).
			camera.OrthoWidth = Properties.GetFloat("camera.ortho.width");
			camera.OrthoHeight = Properties.GetFloat("camera.ortho.height");
			camera.NearPlane = Properties.GetFloat("camera.near.plane");
			camera.FarPlane = Properties.GetFloat("camera.far.plane");
			camera.PerspectiveFov = Properties.GetFloat("camera.fov");

			MessageSystem.Subscribe(this, CoreMessageTypes.Input, (messageType, data, dt) =>
			{
				ProcessInput((FullInputData)data);
			});
		}

		public float Pitch => pitch;
		public float Yaw => yaw;

		public List<MessageHandle> MessageHandles { get; set; }

		public void Dispose()
		{
			MessageSystem.Unsubscribe(this);
		}

		private void ProcessInput(FullInputData data)
		{
			ProcessMouse((MouseData)data.GetData(InputTypes.Mouse));
		}

		private void ProcessMouse(MouseData data)
		{
			ivec2 delta = data.Location - data.PreviousLocation;

			if (settings.InvertX)
			{
				delta.x *= -1;
			}

			if (!settings.InvertY)
			{
				delta.y *= -1;
			}

			var sensitivity = settings.MouseSensitivity / AimDivisor;

			pitch += delta.y * sensitivity;
			pitch = Utilities.Clamp(pitch, -maxPitch, maxPitch);
			yaw += delta.x * sensitivity;

			if (yaw >= Constants.TwoPi)
			{
				yaw -= Constants.TwoPi;
			}
			else if (yaw <= -Constants.TwoPi)
			{
				yaw += Constants.TwoPi;
			}
		}

		public override void Update(float dt)
		{
			// TODO: Since the camera is shifted slighty upward, experiment with subtly modifying follow distance based on pitch.
			quat aim = quat.FromAxisAngle(pitch, vec3.UnitX) * quat.FromAxisAngle(yaw, vec3.UnitY);
			vec3 eye = player.Position + new vec3(0, 0, -followDistance) * aim + new vec3(0, Shift, 0);

			Camera.Position = eye;
			Camera.Orientation = mat4.LookAt(eye, player.Position, vec3.UnitY).ToQuaternion;
		}
	}
}
