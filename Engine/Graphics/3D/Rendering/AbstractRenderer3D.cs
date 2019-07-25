using System;
using System.Collections.Generic;
using Engine.Interfaces._3D;
using Engine.Lighting;
using GlmSharp;

namespace Engine.Graphics._3D.Rendering
{
	public abstract class AbstractRenderer3D<T> : IDisposable where T : IRenderable3D
	{
		protected AbstractRenderer3D(GlobalLight light)
		{
			Light = light;
		}

		protected GlobalLight Light { get; }

		public abstract List<T> RetrieveNext();

		public abstract void Add(T item);
		public abstract void Remove(T item);
		public abstract void Prepare();
		public abstract void PrepareShadow();
		public abstract void Dispose();
		public abstract void Draw(T item, mat4? vp);
	}
}
