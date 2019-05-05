using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeldo.Data
{
	[Flags]
	public enum TileFlags
	{
		Empty = 1<<0,
		Ice = 1<<1,
		Solid = 1<<2
	}
}
