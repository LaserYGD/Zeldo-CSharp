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

namespace Engine.Graphics._3D.Renderers
{
	public class MasterRenderer3D : IRenderTargetUser3D
	{
		private Shader shadowMapShader;
		private RenderTarget shadowMapTarget;
		private mat4 lightMatrix;
		private Camera3D camera;

		private ModelRenderer modelRenderer;
		private SpriteBatch3D spriteBatch3D;

		public MasterRenderer3D(Camera3D camera)
		{
			this.camera = camera;

			shadowMapShader = new Shader();
			shadowMapShader.Attach(ShaderTypes.Vertex, "ShadowMap.vert");
			shadowMapShader.Attach(ShaderTypes.Fragment, "ShadowMap.frag");
			shadowMapShader.AddAttribute<float>(3, GL_FLOAT, false, false, sizeof(float) * 5);
			shadowMapShader.CreateProgram();
			shadowMapShader.Bind(bufferId, indexId);

			int size = Properties.GetInt("shadow.map.size");

			shadowMapTarget = new RenderTarget(size, size, RenderTargetFlags.Depth);
			modelRenderer = new ModelRenderer();
			spriteBatch3D = new SpriteBatch3D();

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

			bool meshExists = handles.TryGetValue(mesh, out var handle);

			if (!meshExists)
			{
				handle = new MeshHandle(indices.Length, indexBufferSize, maxIndex);
				handles.Add(mesh, handle);
			}

			handle.Models.Add(model);

			if (meshExists)
			{
				return;
			}

			maxIndex += mesh.MaxIndex + 1;

			glBindBuffer(GL_ARRAY_BUFFER, bufferId);

			fixed (float* address = &buffer[0])
			{
				glBufferSubData(GL_ARRAY_BUFFER, bufferSize, (uint)size, address);
			}

			bufferSize += size;

			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);

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
			shadowMapShader.Dispose();
			shadowMapTarget.Dispose();
			modelRenderer.Dispose();
			spriteBatch3D.Dispose();
		}

		public void DrawTargets()
		{
			if (!IsEnabled)
			{
				return;
			}

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

			DrawShadow(modelRenderer);
			DrawShadow(spriteBatch3D);
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

		private void DrawShadow<K, V>(AbstractRenderer3D<K, V> renderer) where V : IRenderable3D
		{
			renderer.PrepareShadow();

			List<V> list;

			while ((list = renderer.RetrieveNext()) != null)
			{
				foreach (V item in list)
				{
					// Even if the object doesn't cast a shadow, its world matrix is recomputed here for use during
					// normal rendering.
					item.RecomputeWorldMatrix();

					if (!item.IsShadowCaster)
					{
						continue;
					}

					shadowMapShader.SetUniform("lightMatrix", lightMatrix * item.WorldMatrix);
				}
			}
		}

		public void Draw()
		{
			if (!IsEnabled)
			{
				return;
			}

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

			foreach (MeshHandle handle in handles.Values)
			{
				foreach (Model model in handle.Models)
				{
					mat4 world = model.WorldMatrix;
					quat orientation = model.Orientation;

					modelShader.SetUniform("orientation", orientation.ToMat4);
					modelShader.SetUniform("mvp", ViewProjection * world);
					modelShader.SetUniform("lightBiasMatrix", biasMatrix * lightMatrix * world);

					Draw(handle);
				}
			}
		}

		private void Draw<K, V>(AbstractRenderer3D<K, V> renderer) where V : IRenderable3D
		{
			renderer.Prepare();

			List<V> list;

			while ((list = renderer.RetrieveNext()) != null)
			{
				foreach (V item in list)
				{
					// Even if the object doesn't cast a shadow, its world matrix is recomputed here for use during
					// normal rendering.
					item.RecomputeWorldMatrix();

					if (!item.IsShadowCaster)
					{
						continue;
					}

					shadowMapShader.SetUniform("lightMatrix", lightMatrix * item.WorldMatrix);
				}
			}
		}

		private unsafe void Draw(MeshHandle handle)
		{
			glDrawElementsBaseVertex(GL_TRIANGLES, (uint)handle.Count, GL_UNSIGNED_SHORT, (void*)handle.Offset,
				handle.BaseVertex);
		}
	}
}
