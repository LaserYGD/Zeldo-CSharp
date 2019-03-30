using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Utility;
using Newtonsoft.Json.Linq;

namespace Zeldo.Items
{
	public abstract class Item
	{
		private static Dictionary<int, ItemData> dataMap;

		static Item()
		{
			dataMap = new Dictionary<int, ItemData>();

			var blocks = JsonUtilities.Deserialize<JObject[]>("Items.json");

			foreach (JObject block in blocks)
			{
				int id = block["Id"].Value<int>();
				int value = block["Value"].Value<int>();

				string name = block["Name"].Value<string>();

				dataMap.Add(id, new ItemData(name, value));
			}
		}
	}
}
