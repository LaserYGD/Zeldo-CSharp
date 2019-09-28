
using System.Collections.Generic;
using System.Diagnostics;

namespace Zeldo.Items
{
	public class Inventory
	{
		private HashSet<int> items;

		public Inventory()
		{
			items = new HashSet<int>();
		}

		public void Add(int id)
		{
			Debug.Assert(!items.Contains(id), $"Attempting to add duplicate item to the inventory: {id}");

			// TODO: Trigger the showcase UI to display.
			items.Add(id);
		}
	}
}
