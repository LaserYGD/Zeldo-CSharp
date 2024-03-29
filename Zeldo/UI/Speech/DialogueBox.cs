﻿using System.Collections.Generic;
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
	public class DialogueBox : CanvasElement, IRenderTargetUser2D
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

		public DialogueBox()
		{
			font = ContentCache.GetFont("Default");
			lines = new List<SpriteText>();
			Anchor = Alignments.Bottom;
			Offset = new ivec2(0, 100);
			IsCentered = true;
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

		public void Refresh(DialogueToken token)
		{
			if (!IsVisible)
			{
				IsVisible = true;
			}

			string[] wrapped = Utilities.WrapLines(token.Value, font, Bounds.Width - Padding * 2);

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
			if (!timer.IsPaused)
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
