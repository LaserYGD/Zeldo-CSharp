using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeldo.Items
{
	public class ItemData
	{
		public ItemData(string name, int value)
		{
			Name = name;
			Value = value;
		}

		public string Name { get; }

		public int Value { get; }
	}
}
