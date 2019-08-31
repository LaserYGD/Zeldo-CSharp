using System;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using GlmSharp;

namespace Engine.UI
{
	public abstract class CanvasElement : IBoundable2D, IDynamic, IRenderable2D, IDisposable
	{
		protected Bounds2D bounds;

		protected CanvasElement()
		{
			Components = new ComponentCollection();
			bounds = new Bounds2D();
			IsVisible = true;
		}

		protected ComponentCollection Components { get; }

		public virtual ivec2 Location
		{
			get => bounds.Location;
			set => bounds.Location = value;
		}

		public bool IsVisible { get; set; }
		public bool IsCentered { get; protected set; }
		public bool IsMarkedForDestruction { get; protected set; }
		public bool UsesRenderTarget { get; protected set; }

		public Alignments Anchor { get; set; }

		public ivec2 Offset { get; set; }
		public Bounds2D Bounds => bounds;
		public Canvas Canvas { get; set; }

		public int Width
		{
			get => bounds.Width;
			set => bounds.Width = value;
		}

		public int Height
		{
			get => bounds.Height;
			set => bounds.Height = value;
		}

		public virtual void Dispose()
		{
		}

		public virtual void Update(float dt)
		{
			Components.Update(dt);
		}

		public abstract void Draw(SpriteBatch sb);
	}
}
