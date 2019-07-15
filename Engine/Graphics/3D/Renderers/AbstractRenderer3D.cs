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

		public List<V> RetrieveNext()
		{
			if (nextIndex < keys.Count)
			{
				return Map[keys[nextIndex++]];
			}

			nextIndex = 0;

			return null;
		}

		public abstract void PrepareShadow();
		public abstract void Prepare();
		public abstract void Dispose();
		public abstract void Draw(K key);
	}
}
