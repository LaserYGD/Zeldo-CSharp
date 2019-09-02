using System.Collections.Generic;
using Engine.Interfaces._3D;
using Engine.Lighting;

namespace Engine.Graphics._3D.Rendering
{
	public abstract class MapRenderer3D<K, V> : AbstractRenderer3D<V> where V : IRenderable3D
	{
		private int nextIndex;

		private List<K> keys;

		protected MapRenderer3D(GlobalLight light) : base(light)
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

		public override List<V> RetrieveNext()
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
	}
}
