using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Input.Data;

namespace Zeldo.Entities
{
	public class PlayerControls
	{
		public PlayerControls()
		{
			Attack = new List<InputBind>();
		}

		public List<InputBind> Attack { get; }
	}
}
