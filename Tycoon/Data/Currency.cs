using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tycoon.Data
{
	public class Currency
	{
		public Currency(CurrencyTypes type, int value = 0)
		{
			Type = type;
			Value = value;
		}

		public CurrencyTypes Type { get; }

		public int Value { get; set; }
	}
}
