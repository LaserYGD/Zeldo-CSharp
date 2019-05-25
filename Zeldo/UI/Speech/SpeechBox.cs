using System.Collections.Generic;
using Engine;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Interfaces._2D;
using Engine.Shaders;
using Engine.Timing;
using Engine.UI;
using Engine.Utility;
using GlmSharp;
using static Engine.GL;

namespace Zeldo.UI.Speech
{
	public class SpeechBox : CanvasElement, IRenderTargetUser2D
	{
		private const int Padding = 10;

		private SpriteFont font;
		private List<SpriteText> lines;
		private RepeatingTimer timer;

		// Speech boxes can optionally be given custom shaders for both individual characters and the full text.
		private RenderTarget renderTarget;
		private Shader fullShader;
		private Shader glyphShader;
		private Sprite fullSprite;

		public SpeechBox()
		{
			font = ContentCache.GetFont("Default");
			lines = new List<SpriteText>();
			Bounds = new Bounds2D(500, 150);
			Anchor = Alignments.Bottom;
			Offset = new ivec2(0, 100);
			Centered = true;
		}

		public int Width
		{
			set => Bounds.Width = value;
		}

		public int Height
		{
			set => Bounds.Height = value;
		}

		public override void Dispose()
		{
			renderTarget?.Dispose();
		}

		public void Attach(Shader fullShader, Shader glyphShader)
		{
			this.fullShader = fullShader;
			this.glyphShader = glyphShader;

			renderTarget = new RenderTarget(Bounds.Dimensions, RenderTargetFlags.Color);
		}

		public void Refresh(string value)
		{
			if (!Visible)
			{
				Visible = true;
			}

			string[] wrapped = Utilities.WrapLines(value, font, Bounds.Width - Padding * 2);

			lines.Clear();

			vec2 position = Bounds.Location + new vec2(Padding);

			foreach (var line in wrapped)
			{
				SpriteText spriteText = new SpriteText(font, line);
				spriteText.Position = position;

				lines.Add(spriteText);
				position.y += 20;
			}
		}

		public override void Update(float dt)
		{
			if (!timer.Paused)
			{
				timer.Update(dt);
			}
		}

		public void DrawTargets(SpriteBatch sb)
		{
			// If this function is called, it's assumed the render target (and related shaders) were properly set up.
			renderTarget.Apply();
			fullShader.Apply();

			DrawInternal(sb);
		}

		public override void Draw(SpriteBatch sb)
		{
			if (UsesRenderTarget)
			{
				fullSprite.Draw(sb);

				return;
			}

			DrawInternal(sb);
		}

		private void DrawInternal(SpriteBatch sb)
		{
			if (glyphShader != null)
			{
				sb.Apply(glyphShader, GL_TRIANGLE_STRIP);
			}

			sb.Draw(Bounds, Color.White);
			lines.ForEach(l => l.Draw(sb));
		}
	}
}
