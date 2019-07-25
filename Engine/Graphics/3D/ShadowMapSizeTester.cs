using System.Linq;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Graphics._3D.Rendering;
using Engine.Interfaces._2D;
using Engine.Interfaces._3D;
using Engine.Shapes._3D;
using Engine.View;
using GlmSharp;

namespace Engine.Graphics._3D
{
	public class ShadowMapSizeTester : IRenderTargetUser3D, IRenderable2D
	{
		private Camera3D camera;
		private Camera3D localCamera;
		private MasterRenderer3D batch;
		private PrimitiveRenderer3D primitives;
		private RenderTarget renderTarget;
		private Sprite sprite;

		public ShadowMapSizeTester(Camera3D camera, MasterRenderer3D batch)
		{
			this.camera = camera;
			this.batch = batch;

			localCamera = new Camera3D();
			localCamera.Position = vec3.UnitZ * 25 - new vec3(0, 8, 0);
			localCamera.Orientation = quat.FromAxisAngle(0, vec3.UnitX);
			localCamera.OrthoWidth = 64;
			localCamera.OrthoHeight = 36;
			localCamera.NearPlane = 1;
			localCamera.FarPlane = 60;
			localCamera.IsOrthographic = true;

			renderTarget = new RenderTarget(300, 225, RenderTargetFlags.Color | RenderTargetFlags.Depth);
			primitives = new PrimitiveRenderer3D(localCamera, 1000, 100);
			sprite = new Sprite(renderTarget, null, Alignments.Left | Alignments.Top);
			sprite.Mods = SpriteModifiers.FlipVertical;
		}

		public void Dispose()
		{
			renderTarget.Dispose();
		}

		public void DrawTargets()
		{
			renderTarget.Apply();
			localCamera.Update(0);

			vec3[] cameraBox = DrawViewBox(camera.OrthoWidth, camera.OrthoHeight, camera.NearPlane, camera.FarPlane,
				vec3.Zero, camera.Orientation, Color.Green, out vec3 center);

			vec3 light = batch.Light.Direction;
			quat lightOrientation = new quat(mat4.LookAt(vec3.Zero, light, vec3.UnitY));
			vec3[] transformed = cameraBox.Select(p => p * lightOrientation.Inverse).ToArray();

			//localCamera.Position = center - light * 25;
			//localCamera.Orientation = new quat(mat4.LookAt(vec3.Zero, light, vec3.UnitY));

			float left = transformed.Min(p => p.x);
			float right = transformed.Max(p => p.x);
			float top = transformed.Max(p => p.y);
			float bottom = transformed.Min(p => p.y);
			float near = batch.ShadowNearPlane;
			float far = batch.ShadowFarPlane;

			vec3 shadowPosition = center - batch.Light.Direction * (far - near) / 2;

			DrawViewBox(right - left, top - bottom, near, far, shadowPosition, lightOrientation, Color.Yellow,
				out center);

			primitives.Flush();
		}

		private vec3[] DrawViewBox(float orthoWidth, float orthoHeight, float nearPlane, float farPlane, vec3 position,
			quat orientation, Color color, out vec3 center)
		{
			float orthoHalfWidth = orthoWidth / 2;
			float orthoHalfHeight = orthoHeight / 2;

			nearPlane *= -1;
			farPlane *= -1;

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

			for (int i = 0; i < points.Length; i++)
			{
				points[i] = position + points[i] * orientation;
			}

			center = new vec3(0, 0, nearPlane + (farPlane - nearPlane) / 2) * orientation;

			var lines = new Line3D[12];

			for (int i = 0; i < 4; i++)
			{
				lines[i * 3] = new Line3D(points[i], points[(i + 1) % 4]);
				lines[i * 3 + 1] = new Line3D(points[i + 4], points[(i + 1) % 4 + 4]);
				lines[i * 3 + 2] = new Line3D(points[i], points[i + 4]);
			}

			var p1 = position;
			var p2 = position + vec3.UnitZ * nearPlane * orientation;

			foreach (var line in lines)
			{
				primitives.Draw(line, color);
			}

			primitives.DrawLine(p1, p2, color);

			return points;
		}

		public void Draw(SpriteBatch sb)
		{
			sprite.Draw(sb);
		}
	}
}
