using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Input;
using Engine.Input.Data;

namespace Zeldo.UI.Menus
{
	public abstract class Menu
	{
		private ClickableSet itemSet;

		public void ProcessInput(FullInputData data)
		{
			var mouseData = data.GetData(InputTypes.Mouse);

			if (mouseData != null)
			{
				itemSet.ProcessMouse((MouseData)mouseData);
			}
		}

		protected abstract void Submit(int index);
	}
}
