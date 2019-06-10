using System.Collections.Generic;
using System.Linq;
using Engine.Core;
using Engine.Core._2D;
using Engine.Core._3D;
using Engine.Interfaces._3D;
using Engine.Shaders;
using Engine.View;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics._3D
{
	public class ModelBatch : IRenderTargetUser3D
	{
		private Shader modelShader;
		private Shader shadowMapShader;
		private RenderTarget shadowMapTarget;
		private Texture defaultTexture;
		private mat4 lightMatrix;
		private Camera3D camera;
		private List<MeshHandle> handles;

		private uint bufferId;
		private uint indexBufferId;

		// These sizes are updated as data is buffered to the GPU. The data isn't actually stored here.
		private int bufferSize;
		private int indexBufferSize;
		private int maxIndex;

		public ModelBatch(Camera3D camera, int bufferSize, int indexBufferSize)
		{
			const int ShadowMapSize = 2048;

			this.camera = camera;
			
			GLUtilities.AllocateBuffers(bufferSize, indexBufferSize, out bufferId, out indexBufferId, GL_STATIC_DRAW);

			modelShader = new Shader();
			modelShader.Attach(ShaderTypes.Vertex, "ModelShadow.vert");
			modelShader.Attach(ShaderTypes.Fragment, "ModelShadow.frag");
			modelShader.AddAttribute<float>(3, GL_FLOAT);
			modelShader.AddAttribute<float>(2, GL_FLOAT);
			modelShader.AddAttribute<float>(3, GL_FLOAT);
			modelShader.CreateProgram();
			modelShader.Bind(bufferId, indexBufferId);
			modelShader.Use();
			modelShader.SetUniform("shadowSampler", 0);
			modelShader.SetUniform("textureSampler", 1);

			shadowMapShader = new Shader();
			shadowMapShader.Attach(ShaderTypes.Vertex, "ShadowMap.vert");
			shadowMapShader.Attach(ShaderTypes.Fragment, "ShadowMap.frag");
			shadowMapShader.AddAttribute<float>(3, GL_FLOAT, false, false, sizeof(float) * 5);
			shadowMapShader.CreateProgram();
			shadowMapShader.Bind(bufferId, indexBufferId);

			shadowMapTarget = new RenderTarget(ShadowMapSize, ShadowMapSize, RenderTargetFlags.Depth);
			defaultTexture = ContentCache.GetTexture("Grey.png");
			handles = new List<MeshHandle>();

			// These default values are arbitrary, just to make sure something shows up.
			LightDirection = vec3.UnitX;
			LightColor = Color.White;
			AmbientIntensity = 0.1f;
			IsEnabled = true;
		}

		public vec3 LightDirection { get; set; }
		public Color LightColor { get; set; }

		// Setting the VP matrix externally allows the same batch of models to be rendered from different perspectives
		// (e.g. reflections in puddles).
		public mat4 ViewProjection { get; set; }

		public float ShadowNearPlane { get; set; }
		public float ShadowFarPlane { get; set; }
		public float AmbientIntensity { get; set; }

		public bool IsEnabled { get; set; }

		public unsafe void Add(Model model)
		{
			Mesh mesh = model.Mesh;

			var points = mesh.Points;
			var source = mesh.Source;
			var normals = mesh.Normals;
			var vertices = mesh.Vertices;

			float[] buffer = new float[vertices.Length * 8];

			for (int i = 0; i < vertices.Length; i++)
			{
				var v = vertices[i];
				var p = points[v.x];
				var s = source[v.y];
				var n = normals[v.z];

				int start = i * 8;

				buffer[start] = p.x;
				buffer[start + 1] = p.y;
				buffer[start + 2] = p.z;
				buffer[start + 3] = s.x;
				buffer[start + 4] = s.y;
				buffer[start + 5] = n.x;
				buffer[start + 6] = n.y;
				buffer[start + 7] = n.z;
			}

			ushort[] indices = mesh.Indices;

			int size = sizeof(float) * buffer.Length;
			int indexSize = sizeof(ushort) * indices.Length;
			
			handles.Add(new MeshHandle(model, indices.Length, indexBufferSize, maxIndex););
			maxIndex += mesh.MaxIndex + 1;

			glBindBuffer(GL_ARRAY_BUFFER, bufferId);

			fixed (float* address = &buffer[0])
			{
				glBufferSubData(GL_ARRAY_BUFFER, bufferSize, (uint)size, address);
			}

			bufferSize += size;

			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexBufferId);

			fixed (ushort* address = &indices[0])
			{
				glBufferSubData(GL_ELEMENT_ARRAY_BUFFER, indexBufferSize, (uint)indexSize, address);
			}

			indexBufferSize += indexSize;
		}

		public void Remove(Model model)
		{
		}

		public void Dispose()
		{
			modelShader.Dispose();
			shadowMapShader.Dispose();
			shadowMapTarget.Dispose();

			GLUtilities.DeleteBuffers(bufferId, indexBufferId);
		}

		public void DrawTargets()
		{
			if (!IsEnabled)
			{
				return;
			}

			glDisable(GL_CULL_FACE);

			vec3 orthoHalfSize = ComputeShadowFrustum(out vec3 cameraCenter) / 2;

			// The light matrix is positioned such that the far plane exactly hits the back side of the camera's view
			// box (from the light's perspective). This allows off-screen objects between the light's origin and the
			// screen to still cast shadows.
			float range = ShadowFarPlane - ShadowNearPlane;
			float offset = range / 2 - orthoHalfSize.z;

			mat4 lightView = mat4.LookAt(cameraCenter - LightDirection * offset, cameraCenter, vec3.UnitY);
			mat4 lightProjection = mat4.Ortho(-orthoHalfSize.x, orthoHalfSize.x, -orthoHalfSize.y, orthoHalfSize.y,
				ShadowNearPlane, ShadowFarPlane);

			lightMatrix = lightProjection * lightView;

			shadowMapTarget.Apply();
			shadowMapShader.Apply();

			foreach (MeshHandle handle in handles)
			{
				Model model = handle.Model;

				model.RecomputeWorldMatrix();
				shadowMapShader.SetUniform("lightMatrix", lightMatrix * model.WorldMatrix.Value);
				Draw(handle);
			}
		}

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

			quat lightInverse = new quat(mat4.LookAt(vec3.Zero, LightDirection, vec3.UnitY)).Inverse;

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

		public void Draw()
		{
			if (!IsEnabled)
			{
				return;
			}

			glEnable(GL_CULL_FACE);
			glCullFace(GL_BACK);
			
			shadowMapTarget.Bind(0);
			defaultTexture.Bind(1);

			modelShader.Apply();
			modelShader.SetUniform("lightDirection", LightDirection);
			modelShader.SetUniform("lightColor", LightColor.ToVec3());
			modelShader.SetUniform("ambientIntensity", AmbientIntensity);

			// See http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-16-shadow-mapping/.
			mat4 biasMatrix = new mat4
			(
				0.5f, 0.0f, 0.0f, 0,
				0.0f, 0.5f, 0.0f, 0,
				0.0f, 0.0f, 0.5f, 0,
				0.5f, 0.5f, 0.5f, 1
			);

			foreach (MeshHandle handle in handles)
			{
				Model model = handle.Model;
				mat4 world = model.WorldMatrix.Value;
				quat orientation = model.Orientation;

				modelShader.SetUniform("orientation", orientation.ToMat4);
				modelShader.SetUniform("mvp", ViewProjection * world);
				modelShader.SetUniform("lightBiasMatrix", biasMatrix * lightMatrix * world);

				Draw(handle);
			}
		}

		private unsafe void Draw(MeshHandle handle)
		{
			glDrawElementsBaseVertex(GL_TRIANGLES, (uint)handle.Count, GL_UNSIGNED_SHORT, (void*)handle.Offset,
				handle.BaseVertex);
		}
	}
}
