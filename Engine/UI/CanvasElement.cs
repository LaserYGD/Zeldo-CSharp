using System;
using System.Collections.Generic;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Props;
using GlmSharp;

namespace Engine.UI
{
	// TODO: Consider attaching 2D components (similar to entities) to make positioning easier (don't need to do rotation probably).
	public abstract class CanvasElement : IBoundable2D, IReloadable, IDynamic, IRenderable2D
	{
		private List<(Component2D Target, ivec2 Location)> attachments;

		protected Bounds2D bounds;

		protected CanvasElement()
		{
			attachments = new List<(Component2D target, ivec2 location)>();
			Components = new ComponentCollection();
			bounds = new Bounds2D();
			IsVisible = true;
		}

		protected ComponentCollection Components { get; }

		public virtual ivec2 Location
		{
			get => bounds.Location;
			set
			{
				bounds.Location = value;
				attachments.ForEach(a => a.Target.Position = value + a.Location);
			}
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

		protected T Attach<T>(T component, ivec2? location = null) where T : Component2D
		{
			attachments.Add((component, location ?? ivec2.Zero));

			return component;
		}

		public virtual void Reload(PropertyAccessor accessor)
		{
		}

		public virtual void Dispose()
		{
		}

		// This can be used to initialize components once other values are set.
		public virtual void Initialize()
		{
		}

		public virtual void Update(float dt)
		{
			Components.Update(dt);
		}

		public abstract void Draw(SpriteBatch sb);
	}
}
