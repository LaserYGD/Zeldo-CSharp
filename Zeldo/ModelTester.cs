using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using Engine.Core._2D;
using Engine.Core._3D;
using Engine.Graphics;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.Shaders;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using static Engine.GL;

namespace Zeldo
{
	public class ModelTester : IDynamic, IRenderTargetUser, IRenderable3D
	{
		private Model model;
		private Shader modelShader;
		private Shader shadowMapShader;
		private RenderTarget shadowMapTarget;
		private Texture defaultTexture;
		private vec3 lightDirection;
		private mat4 lightMatrix;
		private Camera3D camera;

		private uint bufferId;
		private uint indexBufferId;

		private float rotation;

		public unsafe ModelTester(Camera3D camera)
		{
			const int ShadowMapSize = 1024;

			this.camera = camera;

			uint[] buffers = new uint[2];

			fixed (uint* address = &buffers[0])
			{
				glGenBuffers(2, address);
			}

			bufferId = buffers[0];
			indexBufferId = buffers[1];

			modelShader = new Shader();
			modelShader.Attach(ShaderTypes.Vertex, "ModelShadow.vert");
			modelShader.Attach(ShaderTypes.Fragment, "ModelShadow.frag");
			modelShader.AddAttribute<float>(3, GL_FLOAT);
			modelShader.AddAttribute<float>(2, GL_FLOAT);
			modelShader.AddAttribute<float>(3, GL_FLOAT);
			modelShader.CreateProgram();
			modelShader.Bind(bufferId, indexBufferId);

			shadowMapShader = new Shader();
			shadowMapShader.Attach(ShaderTypes.Vertex, "ShadowMap.vert");
			shadowMapShader.Attach(ShaderTypes.Fragment, "ShadowMap.frag");
			shadowMapShader.AddAttribute<float>(3, GL_FLOAT, false, 5);
			shadowMapShader.CreateProgram();
			shadowMapShader.Bind(bufferId, indexBufferId);

			shadowMapTarget = new RenderTarget(ShadowMapSize, ShadowMapSize, RenderTargetFlags.Depth);
			model = new Model("Map");
			defaultTexture = ContentCache.GetTexture("Grey.png");

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

			glBindBuffer(GL_ARRAY_BUFFER, bufferId);

			fixed (float* address = &buffer[0])
			{
				glBufferData(GL_ARRAY_BUFFER, (uint)(sizeof(float) * buffer.Length), address, GL_STATIC_DRAW);
			}

			var indices = mesh.Indices;

			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexBufferId);

			fixed (ushort* address = &indices[0])
			{
				glBufferData(GL_ELEMENT_ARRAY_BUFFER, (uint)(sizeof(ushort) * indices.Length), address,
					GL_STATIC_DRAW);
			}
		}

		public RenderTarget ShadowTarget => shadowMapTarget;
		public vec3 LightDirection => lightDirection;

		public void Update(float dt)
		{
			const int OrthoSize = 12;

			rotation += dt;

			vec2 direction = -Utilities.Direction(rotation);

			lightDirection = Utilities.Normalize(new vec3(direction.x, -0.1f, direction.y));

			mat4 lightView = mat4.LookAt(-lightDirection, vec3.Zero, vec3.UnitY);
			mat4 lightProjection = mat4.Ortho(-OrthoSize, OrthoSize, -OrthoSize, OrthoSize, 0.1f, 100);

			lightMatrix = lightProjection * lightView;
		}

		public void DrawTargets()
		{
			glEnable(GL_CULL_FACE);
			glCullFace(GL_FRONT);

			shadowMapTarget.Apply();
			shadowMapShader.Apply();
			shadowMapShader.SetUniform("lightMatrix", lightMatrix * model.World);

			Draw(model.Mesh);
		}

		public void Draw(Camera3D camera)
		{
			glCullFace(GL_BACK);

			// These two texture binds should be reversed (shadow map to zero and texture to one), but for some bizarre
			// reason, it only works correctly like this. I don't understand why.
			glActiveTexture(GL_TEXTURE1);
			glBindTexture(GL_TEXTURE_2D, shadowMapTarget.Id);
			glActiveTexture(GL_TEXTURE0);
			glBindTexture(GL_TEXTURE_2D, defaultTexture.Id);

			modelShader.Apply();
			modelShader.SetUniform("lightColor", vec3.Ones);
			modelShader.SetUniform("lightDirection", lightDirection);
			modelShader.SetUniform("ambientIntensity", 0.1f);

			// See http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-16-shadow-mapping/.
			mat4 biasMatrix = new mat4
			(
				0.5f, 0.0f, 0.0f, 0,
				0.0f, 0.5f, 0.0f, 0,
				0.0f, 0.0f, 0.5f, 0,
				0.5f, 0.5f, 0.5f, 1
			);

			mat4 cameraMatrix = camera.ViewProjection;
			mat4 world = model.World;
			quat orientation = model.Orientation;

			modelShader.SetUniform("orientation", orientation.ToMat4);
			modelShader.SetUniform("mvp", cameraMatrix * world);
			modelShader.SetUniform("lightBiasMatrix", biasMatrix * lightMatrix * world);

			Draw(model.Mesh);
		}

		private unsafe void Draw(Mesh mesh)
		{
			glDrawElements(GL_TRIANGLES, (uint)mesh.Indices.Length, GL_UNSIGNED_SHORT, null);
		}
	}
}
