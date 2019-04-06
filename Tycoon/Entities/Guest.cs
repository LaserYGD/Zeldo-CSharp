using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Entities;
using Tycoon.Data;

namespace Tycoon.Entities
{
	public class Guest : Entity3D
	{
		public string Name { get; }

		public Species Species { get; }
	}
}
