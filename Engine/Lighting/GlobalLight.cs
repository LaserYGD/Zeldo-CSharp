using System.Linq;
using Engine.Core;
using Engine.Interfaces;
using GlmSharp;

namespace Engine.Lighting
{
	public class GlobalLight : IColorable
	{
		public vec3 Direction { get; set; }
		public Color Color { get; set; }

		public float AmbientIntensity { get; set; }

		public mat4 Matrix { get; private set; }
		public mat4 BiasMatrix { get; private set; }

		public void RecomputeMatrices(mat4 vp)
		{
			const float ShadowNearPlane = 0.1f;
			const float ShadowFarPlane = 100;

			// See http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-16-shadow-mapping/.
			mat4 bias = new mat4
			(
				0.5f, 0.0f, 0.0f, 0,
				0.0f, 0.5f, 0.0f, 0,
				0.0f, 0.0f, 0.5f, 0,
				0.5f, 0.5f, 0.5f, 1
			);

			//vec3 orthoHalfSize = ComputeShadowFrustum(out vec3 cameraCenter) / 2;
			vec3 orthoHalfSize = new vec3(20, 20, 50);
			vec3 cameraCenter = vec3.Zero;

			// The light matrix is positioned such that the far plane exactly hits the back side of the camera's view
			// box (from the light's perspective). This allows off-screen objects between the light's origin and the
			// screen to still cast shadows.
			float halfRange = orthoHalfSize.z / 2;

			mat4 lightView = mat4.LookAt(cameraCenter - Direction * halfRange, cameraCenter, vec3.UnitY);
			mat4 lightProjection = mat4.Ortho(-orthoHalfSize.x, orthoHalfSize.x, -orthoHalfSize.y, orthoHalfSize.y,
				ShadowNearPlane, ShadowFarPlane);

			Matrix = lightProjection * lightView;
			BiasMatrix = bias * Matrix;
		}

		/*
		private vec3 ComputeShadowFrustum(out vec3 cameraCenter)
		{
			float orthoHalfWidth = camera.OrthoWidth / 2;
			float orthoHalfHeight = camera.OrthoHeight / 2;
			float nearPlane = -camera.NearPlane;
			float farPlane = -camera.FarPlane;

			var points = new vec3[8];
			points[0] = new vec3(-orthoHalfWidth, orthoHalfHeight, nearPlane);
			points[1] = new vec3(orthoHalfWidth, orthoHalfHeight, nearPlane);
			points[2] = new vec3(orthoHalfWidth, -orthoHalfHeight, nearPlane);
			points[3] = new vec3(-orthoHalfWidth, -orthoHalfHeight, nearPlane);

			for (int i = 0; i < 4; i++)
			{
				vec3 p = points[i];
				p.z = farPlane;
				points[i + 4] = p;
			}

			quat cameraOrientation = camera.Orientation;

			for (int i = 0; i < points.Length; i++)
			{
				points[i] *= cameraOrientation;
			}

			cameraCenter = camera.Position + new vec3(0, 0, nearPlane + (farPlane - nearPlane) / 2) *
				cameraOrientation;

			quat lightInverse = new quat(mat4.LookAt(vec3.Zero, Direction, vec3.UnitY)).Inverse;

			for (int i = 0; i < points.Length; i++)
			{
				points[i] = points[i] * lightInverse;
			}

			float left = points.Min(p => p.x);
			float right = points.Max(p => p.x);
			float top = points.Max(p => p.y);
			float bottom = points.Min(p => p.y);
			float near = points.Min(p => p.z);
			float far = points.Max(p => p.z);

			return new vec3(right - left, top - bottom, far - near);
		}
		*/
	}
}
