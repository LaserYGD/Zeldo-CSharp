using System;
using System.Collections.Generic;
using Engine.Interfaces._3D;

namespace Engine.Graphics._3D.Renderers
{
	public abstract class AbstractRenderer3D<K, V> : IDisposable where V : IRenderable3D
	{
		private int nextIndex;

		private List<K> keys;

		protected AbstractRenderer3D()
		{
			Map = new Dictionary<K, List<V>>();
			keys = new List<K>();
		}

		protected Dictionary<K, List<V>> Map { get; }

		protected void Add(K key, V item)
		{
			if (!Map.TryGetValue(key, out var list))
			{
				list = new List<V>();
				Map.Add(key, list);
				keys.Add(key);
			}

			list.Add(item);
		}

		protected void Remove(K key, V item)
		{
			Map[key].Remove(item);
		}

		public List<V> RetrieveNext()
		{
			if (nextIndex < keys.Count)
			{
				K key = keys[nextIndex++];

				// In this context, "apply" means binding any relevant open GL state before draw calls begin.
				Apply(key);

				return Map[key];
			}

			// This resets the renderer for the next phase.
			nextIndex = 0;

			return null;
		}

		protected virtual void Apply(K key)
		{
		}

		public abstract void Add(V item);
		public abstract void Remove(V item);
		public abstract void PrepareShadow();
		public abstract void Prepare();
		public abstract void Dispose();
		public abstract void Draw(K key);
	}
}
