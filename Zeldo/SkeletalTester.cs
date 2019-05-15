using System;
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
		private mat4 bone1;
		private mat4 bone2;

		private uint bufferId;
		private uint indexId;

		private float angle;

		public SkeletalTester()
		{
			const int ShadowMapSize = 2048;

			GLUtilities.AllocateBuffers(10000, 1000, out bufferId, out indexId, GL_STATIC_DRAW);

			skeletalShader = new Shader();
			skeletalShader.Attach(ShaderTypes.Vertex, "Skeletal.vert");
			skeletalShader.Attach(ShaderTypes.Fragment, "ModelShadow.frag");
			skeletalShader.AddAttribute<float>(3, GL_FLOAT);
			skeletalShader.AddAttribute<float>(2, GL_FLOAT);
			skeletalShader.AddAttribute<float>(3, GL_FLOAT);
			skeletalShader.AddAttribute<float>(2, GL_FLOAT);
			skeletalShader.AddAttribute<short>(2, GL_SHORT, true);
			skeletalShader.AddAttribute<int>(1, GL_INT, true);
			skeletalShader.CreateProgram();
			skeletalShader.Bind(bufferId, indexId);
			skeletalShader.Use();
			skeletalShader.SetUniform("shadowSampler", 0);
			skeletalShader.SetUniform("textureSampler", 1);

			shadowMapShader = new Shader();
			shadowMapShader.Attach(ShaderTypes.Vertex, "ShadowMapSkeletal.vert");
			shadowMapShader.Attach(ShaderTypes.Fragment, "ShadowMap.frag");
			shadowMapShader.AddAttribute<float>(3, GL_FLOAT, false, false, 20);
			shadowMapShader.AddAttribute<float>(2, GL_FLOAT);
			shadowMapShader.AddAttribute<short>(2, GL_SHORT, true);
			shadowMapShader.AddAttribute<int>(1, GL_INT, true);
			shadowMapShader.CreateProgram();
			shadowMapShader.Bind(bufferId, indexId);

			shadowMapTarget = new RenderTarget(ShadowMapSize, ShadowMapSize, RenderTargetFlags.Depth);
			defaultTexture = ContentCache.GetTexture("Grey.png");
			lightDirection = -vec3.UnitY;

			model = new Model("Tree.dae");

			BufferMesh(model.Mesh);
		}

		private unsafe void BufferMesh(Mesh mesh)
		{
			const int VertexSize = sizeof(float) * 10 + sizeof(int) * 2;
			
			var points = mesh.Points;
			var source = mesh.Source;
			var normals = mesh.Normals;
			var vertices = mesh.Vertices;
			var boneIndexes = mesh.BoneIndexes;
			var boneWeights = mesh.BoneWeights;

			byte[] buffer = new byte[VertexSize * vertices.Length];

			for (int i = 0; i < vertices.Length; i++)
			{
				var v = vertices[i];
				var p = points[v.x];
				var s = source[v.y];
				var n = normals[v.z];
				var bW = boneWeights[i];
				var bI = boneIndexes[i];

				int start = VertexSize * i;

				// Buffer floats.
				float[] floats =
				{
					p.x,
					p.y,
					p.z,
					s.x,
					s.y,
					n.x,
					n.y,
					n.z,
					bW.x,
					bW.y
				};

				Buffer.BlockCopy(floats, 0, buffer, start, 40);

				// Buffer shorts.
				short[] shorts =
				{
					(short)bI.x,
					(short)bI.y
				};
				
				Buffer.BlockCopy(shorts, 0, buffer, start + 40, 4);

				// Buffer integers.
				int boneCount = bI.y == -1 ? 1 : 2;

				Buffer.BlockCopy(BitConverter.GetBytes(boneCount), 0, buffer, start + 44, 4);
			}

			ushort[] indices = mesh.Indices;

			glBindBuffer(GL_ARRAY_BUFFER, bufferId);

			fixed (byte* address = &buffer[0])
			{
				glBufferSubData(GL_ARRAY_BUFFER, 0, (uint)buffer.Length, address);
			}

			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);

			fixed (ushort* address = &indices[0])
			{
				glBufferSubData(GL_ELEMENT_ARRAY_BUFFER, 0, (uint)(sizeof(ushort) * indices.Length), address);
			}
		}

		public void Update(float dt)
		{
			angle += dt / 2;
			
			bone1 = quat.FromAxisAngle(angle, vec3.UnitY).ToMat4;
			bone2 = quat.Identity.ToMat4;
		}

		public void DrawTargets()
		{
			const int OrthoSize = 8;

			glDisable(GL_CULL_FACE);

			mat4 lightView = mat4.LookAt(-lightDirection * 10, vec3.Zero, vec3.UnitY);
			mat4 lightProjection = mat4.Ortho(-OrthoSize, OrthoSize, -OrthoSize, OrthoSize, 0.1f, 100);
			
			mat4[] bones =
			{
				bone1,
				bone2
			};

			lightMatrix = lightProjection * lightView;

			shadowMapTarget.Apply();
			shadowMapShader.Apply();
			shadowMapShader.SetUniform("bones[0]", bones);

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

			mat4[] bones =
			{
				bone1,
				bone2
			};

			skeletalShader.Apply();
			skeletalShader.SetUniform("lightDirection", lightDirection);
			skeletalShader.SetUniform("lightColor", Color.White.ToVec3());
			skeletalShader.SetUniform("ambientIntensity", 0.1f);
			skeletalShader.SetUniform("bones[0]", bones);

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

			Draw(model.Mesh);
		}

		private unsafe void Draw(Mesh mesh)
		{
			glDrawElements(GL_TRIANGLES, (uint)mesh.Indices.Length, GL_UNSIGNED_SHORT, null);
		}
	}
}
