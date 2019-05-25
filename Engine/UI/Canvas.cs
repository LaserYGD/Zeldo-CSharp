﻿using System.Collections.Generic;
using System.Linq;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Messaging;
using GlmSharp;

namespace Engine.UI
{
	public class Canvas : IReceiver, IDynamic, IRenderTargetUser2D, IRenderable2D
	{
		private List<CanvasElement> elements;
		private List<IRenderTargetUser2D> renderTargetUsers;

		public Canvas()
		{
			elements = new List<CanvasElement>();
			renderTargetUsers = new List<IRenderTargetUser2D>();

			MessageSystem.Subscribe(this, CoreMessageTypes.ResizeWindow, (messageType, data, dt) =>
			{
				elements.ForEach(PlaceElement);
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }

		public void Dispose()
		{
			elements.ForEach(e => e.Dispose());

			MessageSystem.Unsubscribe(this);
		}

		public void Add(CanvasElement element)
		{
			elements.Add(element);
			PlaceElement(element);

			if (element.UsesRenderTarget)
			{
				renderTargetUsers.Add((IRenderTargetUser2D)element);
			}
		}

		public void Remove(CanvasElement element)
		{
			elements.Remove(element);
		}

		public T GetElement<T>() where T : CanvasElement
		{
			return elements.OfType<T>().First();
		}

		private void PlaceElement(CanvasElement element)
		{
			Alignments anchor = element.Anchor;

			bool left = (anchor & Alignments.Left) > 0;
			bool right = (anchor & Alignments.Right) > 0;
			bool top = (anchor & Alignments.Top) > 0;
			bool bottom = (anchor & Alignments.Bottom) > 0;

			ivec2 dimensions = Resolution.WindowDimensions;
			ivec2 offset = element.Offset;

			int width = dimensions.x;
			int height = dimensions.y;
			int x = left ? offset.x : (right ? width - offset.x : width / 2 + offset.x);
			int y = top ? offset.y : (bottom ? height - offset.y : height / 2 + offset.y);

			element.Location = new ivec2(x, y);
		}

		public void Update(float dt)
		{
			foreach (CanvasElement element in elements)
			{
				if (element.Visible)
				{
					element.Update(dt);
				}
			}
		}

		public void DrawTargets(SpriteBatch sb)
		{
			renderTargetUsers.ForEach(u => u.DrawTargets(sb));
		}

		public void Draw(SpriteBatch sb)
		{
			foreach (CanvasElement element in elements)
			{
				if (element.Visible)
				{
					element.Draw(sb);
				}
			}
		}
	}
}
