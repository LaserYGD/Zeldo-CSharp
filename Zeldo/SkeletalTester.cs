using Engine;
using Engine.Core;
using Engine.Core._2D;
using Engine.Core._3D;
using Engine.Graphics;
using Engine.Graphics._3D;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.Shaders;
using Engine.Utility;
using Engine.View;
using GlmSharp;
using static Engine.GL;

namespace Zeldo
{
	public class SkeletalTester : IDynamic, IRenderTargetUser, IRenderable3D
	{
		private Model model;
		private Shader skeletalShader;
		private Shader shadowMapShader;
		private RenderTarget shadowMapTarget;
		private Texture defaultTexture;
		private mat4 lightMatrix;
		private vec3 lightDirection;

		private uint bufferId;
		private uint indexId;

		public SkeletalTester()
		{
			const int ShadowMapSize = 2048;

			GLUtilities.AllocateBuffers(10000, 1000, out bufferId, out indexId, GL_STATIC_DRAW);

			skeletalShader = new Shader();
			skeletalShader.Attach(ShaderTypes.Vertex, "Skeletal.vert");
			skeletalShader.Attach(ShaderTypes.Fragment, "ModelShadow.vert");
			skeletalShader.AddAttribute<float>(3, GL_FLOAT);
			skeletalShader.AddAttribute<float>(2, GL_FLOAT);
			skeletalShader.AddAttribute<float>(3, GL_FLOAT);
			skeletalShader.AddAttribute<byte>(2, GL_BYTE, true);
			skeletalShader.AddAttribute<float>(2, GL_FLOAT);
			skeletalShader.CreateProgram();
			skeletalShader.Bind(bufferId, indexId);
			skeletalShader.Use();
			skeletalShader.SetUniform("shadowSampler", 0);
			skeletalShader.SetUniform("textureSampler", 1);

			shadowMapShader = new Shader();
			shadowMapShader.Attach(ShaderTypes.Vertex, "ShadowMap.vert");
			shadowMapShader.Attach(ShaderTypes.Fragment, "ShadowMap.frag");
			shadowMapShader.AddAttribute<float>(3, GL_FLOAT, false, sizeof(float) * 2);
			shadowMapShader.AddAttribute<byte>(2, GL_BYTE, true);
			shadowMapShader.AddAttribute<float>(2, GL_FLOAT);
			shadowMapShader.CreateProgram();
			shadowMapShader.Bind(bufferId, indexId);

			shadowMapTarget = new RenderTarget(ShadowMapSize, ShadowMapSize, RenderTargetFlags.Depth);
			defaultTexture = ContentCache.GetTexture("Grey.png");
			lightDirection = Utilities.Normalize(new vec3(1, -0.5f, -0.25f));

			model = new Model("Tree.dae");
			model.Scale = new vec3(0.75f);
		}

		public void Update(float dt)
		{
		}

		public void DrawTargets()
		{
			const int OrthoSize = 8;

			glDisable(GL_CULL_FACE);

			mat4 lightView = mat4.LookAt(-lightDirection * 10, vec3.Zero, vec3.UnitY);
			mat4 lightProjection = mat4.Ortho(-OrthoSize, OrthoSize, -OrthoSize, OrthoSize, 0.1f, 100);

			vec2[] bones =
			{
				vec2.Ones,
				vec2.Ones
			};

			lightMatrix = lightProjection * lightView;

			shadowMapTarget.Apply();
			shadowMapShader.Apply();
			//shadowMapShader.SetUniform("bones", bones);

			model.RecomputeWorldMatrix();
			shadowMapShader.SetUniform("lightMatrix", lightMatrix * model.WorldMatrix);
			Draw(model.Mesh);
		}

		public void Draw(Camera3D camera)
		{
			glEnable(GL_CULL_FACE);
			glCullFace(GL_BACK);
			glActiveTexture(GL_TEXTURE0);
			glBindTexture(GL_TEXTURE_2D, shadowMapTarget.Id);
			glActiveTexture(GL_TEXTURE1);
			glBindTexture(GL_TEXTURE_2D, defaultTexture.Id);

			skeletalShader.Apply();
			skeletalShader.SetUniform("lightDirection", lightDirection);
			skeletalShader.SetUniform("lightColor", Color.White.ToVec3());
			skeletalShader.SetUniform("ambientIntensity", 0.1f);
			//shadowMapShader.SetUniform("bones", bones);

			// See http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-16-shadow-mapping/.
			mat4 biasMatrix = new mat4
			(
				0.5f, 0.0f, 0.0f, 0,
				0.0f, 0.5f, 0.0f, 0,
				0.0f, 0.0f, 0.5f, 0,
				0.5f, 0.5f, 0.5f, 1
			);

			mat4 cameraMatrix = camera.ViewProjection;
			mat4 world = model.WorldMatrix;
			quat orientation = model.Orientation;

			skeletalShader.SetUniform("orientation", orientation.ToMat4);
			skeletalShader.SetUniform("mvp", cameraMatrix * world);
			skeletalShader.SetUniform("lightBiasMatrix", biasMatrix * lightMatrix * world);
		}

		private unsafe void Draw(Mesh mesh)
		{
			glDrawElements(GL_TRIANGLES, (uint)mesh.Indices.Length, GL_UNSIGNED_SHORT, null);
		}
	}
}
