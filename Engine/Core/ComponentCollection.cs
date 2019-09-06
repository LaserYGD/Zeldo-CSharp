using System.Collections.Generic;
using Engine.Interfaces;

namespace Engine.Core
{
	public class ComponentCollection : IDynamic
	{
		private List<IComponent> components = new List<IComponent>();

		// Returning the component allows for chained function calls if desired.
		public T Add<T>(T component) where T : IComponent
		{
			components.Add(component);

			return component;
		}

		public void Remove(IComponent component)
		{
			components.Remove(component);
		}

		public void Update(float dt)
		{
			for (int i = components.Count - 1; i >= 0; i--)
			{
				var component = components[i];
				component.Update(dt);

				if (component.IsComplete)
				{
					components.RemoveAt(i);
				}
			}
		}
	}
}
