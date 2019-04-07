using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;

namespace Tycoon.Tools
{
	public class Toolbox : IReceiver
	{
		public Toolbox()
		{
			MessageHandles = new List<MessageHandle>();

			MessageSystem.Subscribe(this, CoreMessageTypes.Input, (messageType, data, dt) =>
			{
				ProcessInput((FullInputData)data);
			});
		}

		public List<MessageHandle> MessageHandles { get; set; }

		private void ProcessInput(FullInputData data)
		{
		}
	}
}
