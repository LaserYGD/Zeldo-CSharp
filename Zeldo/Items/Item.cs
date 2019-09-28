using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Utility;

namespace Zeldo.Items
{
	public abstract class Item
	{
		private static Dictionary<int, ItemData> dataMap;

		static Item()
		{
			var data = JsonUtilities.Deserialize<ItemData[]>("Items.json");
			dataMap = data.ToDictionary(d => d.Id, d => d);
		}

		private ItemData data;

		protected Item(int id)
		{
			Debug.Assert(dataMap.ContainsKey(id), $"Missing data for item {id}.");

			data = dataMap[id];
		}
	}
}
