using System.Collections.Generic;
using Engine.Interfaces;

namespace Engine.Core
{
	public class ComponentCollection : IDynamic
	{
		private List<DynamicComponent> components = new List<DynamicComponent>();

		public void Add(DynamicComponent component)
		{
			components.Add(component);
		}

		public void Update(float dt)
		{
			for (int i = components.Count - 1; i >= 0; i--)
			{
				var component = components[i];
				component.Update(dt);

				if (component.Complete)
				{
					components.RemoveAt(i);
				}
			}
		}
	}
}
