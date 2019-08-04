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
		private ivec2 location;

		protected CanvasElement()
		{
			Components = new ComponentCollection();
			Visible = true;
		}

		protected ComponentCollection Components { get; }

		protected bool Centered { get; set; }

		public virtual ivec2 Location
		{
			get => location;
			set
			{
				location = value;

				if (Bounds == null)
				{
					return;
				}

				if (Centered)
				{
					Bounds.Center = value;
				}
				else
				{
					Bounds.Location = value;
				}
			}
		}

		public bool Visible { get; set; }
		public bool UsesRenderTarget { get; protected set; }
		public bool MarkedForDestruction { get; protected set; }

		public Alignments Anchor { get; set; }

		public ivec2 Offset { get; set; }
		public Bounds2D Bounds { get; protected set; }
		public Canvas Canvas { get; set; }

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
