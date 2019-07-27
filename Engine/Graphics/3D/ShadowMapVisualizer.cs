using System.Collections.Generic;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Shaders;
using Engine.UI;
using GlmSharp;
using static Engine.GL;

namespace Engine.Graphics._3D
{
	public class ShadowMapVisualizer : CanvasElement, IReceiver
	{
		private const int DefaultSize = 250;

		private Sprite sprite;

		public ShadowMapVisualizer(RenderTarget shadowMapTarget)
		{
			Shader shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "Sprite.vert");
			shader.Attach(ShaderTypes.Fragment, "ShadowMapVisualization.frag");
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<byte>(4, GL_UNSIGNED_BYTE, ShaderAttributeFlags.IsNormalized);
			shader.Initialize();

			sprite = new Sprite(shadowMapTarget, null, Alignments.Left | Alignments.Bottom);
			sprite.Shader = shader;
			//sprite.Color = Color.Yellow;

			DisplaySize = DefaultSize;

			MessageSystem.Subscribe(this, CoreMessageTypes.ResizeWindow, (messageType, data, dt) =>
			{
				sprite.Position = new vec2(0, Resolution.WindowHeight);
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }

		public int DisplaySize
		{
			set => sprite.ScaleTo(value, value);
		}

		public override void Dispose()
		{
			sprite.Dispose();

			MessageSystem.Unsubscribe(this);
		}

		public override void Draw(SpriteBatch sb)
		{
			sprite.Draw(sb);
		}
	}
}
