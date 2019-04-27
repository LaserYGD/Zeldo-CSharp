﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using GlmSharp;

namespace Engine.UI
{
	public abstract class CanvasElement : ILocatable, IDynamic, IRenderable2D
	{
		protected CanvasElement()
		{
			Visible = true;
		}

		public virtual ivec2 Location { get; set; }

		public bool Visible { get; set; }

		public Alignments Anchor { get; set; }

		public ivec2 Offset { get; set; }

		public virtual void Update(float dt)
		{
		}

		public abstract void Draw(SpriteBatch sb);
	}
}
