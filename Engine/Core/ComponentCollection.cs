using System.Collections.Generic;
using Engine.Interfaces;

namespace Engine.Core
{
	public class ComponentCollection : IDynamic
	{
		private List<Component> components = new List<Component>();

		public void Add(Component component)
		{
			components.Add(component);
		}

		public void Remove(Component component)
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
