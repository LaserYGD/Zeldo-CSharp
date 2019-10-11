using System.Collections.Generic;
using System.Diagnostics;
using Engine.Interfaces;

namespace Engine.Core
{
	public class ComponentCollection : IDynamic
	{
		private List<IComponent> components = new List<IComponent>();

		// Returning the component allows for chained function calls (if desired).
		public T Add<T>(T component) where T : IComponent
		{
			Debug.Assert(component != null, "Can't add a null component.");

			components.Add(component);

			return component;
		}

		public void Remove(IComponent component)
		{
			Debug.Assert(components.Contains(component), "The given component was either not added or already removed.");

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
