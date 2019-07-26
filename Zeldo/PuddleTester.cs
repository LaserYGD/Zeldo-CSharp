using Engine;
using Engine.Core._2D;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.Shaders;
using Engine.View;
using GlmSharp;
using Zeldo.Entities.Core;

namespace Zeldo
{
	public class PuddleTester : IDynamic, IRenderTargetUser3D
	{
		private RenderTarget modelTarget;
		private Shader shader;

		private mat4 viewOrientation;

		public PuddleTester()
		{
			modelTarget = new RenderTarget(Resolution.RenderWidth, Resolution.RenderHeight, RenderTargetFlags.Color |
				RenderTargetFlags.Depth);

			shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "");
			shader.Attach(ShaderTypes.Fragment, "Puddle.frag");
			shader.Initialize();
			shader.Use();
			shader.SetUniform("skyImage", 0);
			shader.SetUniform("modelImage", 1);
		}

		public Sky Sky { get; set; }
		public Scene Scene { get; set; }
		public Camera3D Camera
		{
			set
			{
				// This reflects the angle off flat ground.
				float reflected = (float)-value.Orientation.Pitch;

				viewOrientation = mat4.Rotate(reflected, vec3.UnitX);
			}
		}

		public void Dispose()
		{
			modelTarget.Dispose();
			shader.Dispose();
		}

		public void Update(float dt)
		{
		}

		public void DrawTargets()
		{
			modelTarget.Apply();

			// Note that by this point, the scene's shadow map should have already been computed.
			var renderer = Scene.Renderer;
			renderer.VpMatrix = viewOrientation;
			renderer.Draw();
			
			shader.Apply();
			Sky.Target.Bind(0);
			modelTarget.Bind(1);
		}

		public void Draw(Camera3D camera)
		{
		}
	}
}
